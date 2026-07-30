using UnityEngine;
using Unity.Services.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using Random = UnityEngine.Random;

public class SessionManager : MonoBehaviour
{
    public event Action OnAuthenticatedSucceed;
    public event Action OnAuthenticatedFailed;
    public event Action<ISession> OnSessionConnected;
    public event Action OnSessionEnded;
    public event Action<string> OnSessionHostUpdated;
    public event Action<LobbyData> OnSessionUpdate;
    public event Action OnSessionLostConnection;

    private bool isCreatingSession;
    private bool isLeavingSession;

    private ISession currentSession;
    public ISession CurrentSession => currentSession;

    private void Awake()
    {
        InitializeAsync();
    }

    private void Start()
    {
        NetworkManager.Singleton.OnConnectionEvent += SingletonOnOnConnectionEvent;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnConnectionEvent -= SingletonOnOnConnectionEvent;
        }
    }

    private void SingletonOnOnConnectionEvent(NetworkManager networkManager, ConnectionEventData eventData)
    {
        switch (eventData.EventType)
        {
            case ConnectionEvent.ClientDisconnected:
                if (isLeavingSession == true)
                {
                    return;
                }

                OnSessionLostConnection?.Invoke();

                break;
        }
    }

    private async void InitializeAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            OnAuthenticatedSucceed?.Invoke();
        }
        catch (Exception e)
        {
            OnAuthenticatedFailed?.Invoke();
            Debug.LogException(e);
        }
    }

    #region Create Session

    public async Task CreateSessionAsHost(SessionOptions options)
    {
        if (CanCreateSession() == false)
        {
            return;
        }

        isCreatingSession = true;

        try
        {
            currentSession = await MultiplayerService.Instance.CreateSessionAsync(options);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            isCreatingSession = false;
            OnSessionConnected?.Invoke(currentSession);
        }
    }

    #endregion

    #region Join Session

    public async Task JoinSessionByCode(string joinCode)
    {
        try
        {
            currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode);
            OnSessionConnected?.Invoke(currentSession);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task JoinSessionByID(string joinID)
    {
        try
        {
            currentSession = await MultiplayerService.Instance.JoinSessionByIdAsync(joinID);
            OnSessionConnected?.Invoke(currentSession);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task QuickJoinLobby()
    {
        QuerySessionsOptions querySessionsOptions = new QuerySessionsOptions();
        QuerySessionsResults querySessionsResults = await MultiplayerService.Instance.QuerySessionsAsync(querySessionsOptions);

        ISessionInfo session = querySessionsResults.Sessions[Random.Range(0, querySessionsResults.Sessions.Count)];
        JoinSessionByID(session.Id);
    }

    public async Task RejoinSession()
    {
        try
        {
            if (currentSession != null)
            {
                await currentSession.RefreshAsync();

                if (currentSession.HasPlayer(AuthenticationService.Instance.PlayerId) == true)
                {
                    await currentSession.ReconnectAsync();
                    NetworkManager.Singleton.StartClient();
                    OnSessionConnected?.Invoke(currentSession);

                    return;
                }

                currentSession = null;
            }

            List<string> sessionIds = await MultiplayerService.Instance.GetJoinedSessionIdsAsync();

            if (sessionIds == null || sessionIds.Count == 0)
            {
                return;
            }

            string sessionId = sessionIds[0];
            currentSession = await MultiplayerService.Instance.ReconnectToSessionAsync(sessionId);
            NetworkManager.Singleton.StartClient();
            OnSessionConnected?.Invoke(currentSession);
        }

        catch (SessionException e)
        {
            Debug.Log(e);
        }
    }

    #endregion

    #region Leave Session

    public async Task LeaveSession()
    {
        if (false) // TODO migração de host considerando NGO e relay
        {
            foreach (IReadOnlyPlayer readOnlyPlayer in currentSession.Players)
            {
                if (currentSession.Host == readOnlyPlayer.Id)
                {
                    continue;
                }

                await ElectPlayerAsHostAsync(readOnlyPlayer.Id);
                OnSessionHostUpdated?.Invoke(readOnlyPlayer.Id);

                break;
            }
        }

        LeaveSessionAsClient();
    }

    public async Task LeaveSessionAsClient()
    {
        isLeavingSession = true;
        await currentSession.LeaveAsync();
        isLeavingSession = false;
        currentSession = null;
        OnSessionEnded?.Invoke();
    }

    #endregion

    #region Kick Player

    public void KickPlayerByID(string playerID)
    {
        currentSession.AsHost().RemovePlayerAsync(playerID);
    }

    #endregion

    #region Update Session

    public async Task UpdateLobbyData(bool isPrivate, string lobbyName)
    {
        if (currentSession.IsHost == false)
        {
            return;
        }

        LobbyData newLobby = new LobbyData(currentSession.MaxPlayers, isPrivate, lobbyName, currentSession.Code);
        await UpdateSessionAsync(newLobby);
        OnSessionUpdate?.Invoke(newLobby);
    }

    private async Task UpdateSessionAsync(LobbyData lobbyData)
    {
        try
        {
            IHostSession hostSession = currentSession.AsHost();

            hostSession.Name = lobbyData.LobbyName.Value;
            hostSession.IsPrivate = lobbyData.IsPrivate;

            await hostSession.SavePropertiesAsync();

            Debug.Log(
                $"Sessão atualizada: " +
                $"{hostSession.Name}, " +
                $"privada: {hostSession.IsPrivate}"
            );
        }
        catch (SessionException exception)
        {
            Debug.LogError(
                $"Falha ao atualizar sessão: {exception.Message}"
            );
        }
    }

    #endregion

    #region HostMigration

    public Task ElectPlayerAsHostAsync(string playerId)
    {
        IHostSession hostSession = currentSession.AsHost();
        hostSession.Host = playerId;

        return hostSession.SavePropertiesAsync();
    }

    #endregion

    private bool CanCreateSession()
    {
        if (isCreatingSession == true)
        {
            Debug.LogWarning("A sessionUI is already being created.");

            return false;
        }

        if (currentSession != null)
        {
            Debug.LogWarning("The player is already hosting a session.");

            return false;
        }

        return true;
    }
}
