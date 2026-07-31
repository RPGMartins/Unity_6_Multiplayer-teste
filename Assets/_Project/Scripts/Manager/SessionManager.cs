using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using Random = UnityEngine.Random;

public class SessionManager : MonoBehaviour
{
    public event Action<ISession> OnSessionConnected;
    public event Action<ISession> OnSessionEnded;
    public event Action<ISession> OnSessionLostConnection;
    public event Action<LobbyData> OnSessionUpdate;

    private bool isCreatingSession;
    private bool isLeavingSession;

    private ISession currentSession;
    private AuthenticationManager authenticationManager;
    public ISession CurrentSession => currentSession;

    private void Start()
    {
        authenticationManager = FindAnyObjectByType<AuthenticationManager>(); //TODO USAR SINGLETON
        NetworkManager.Singleton.OnConnectionEvent += HandleConnection;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnConnectionEvent -= HandleConnection;
        }
    }

    private void HandleConnection(NetworkManager networkManager, ConnectionEventData eventData)
    {
        switch (eventData.EventType)
        {
            case ConnectionEvent.ClientDisconnected:
                if (isLeavingSession == true)
                {
                    return;
                }

                OnSessionLostConnection?.Invoke(currentSession);

                break;
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
            OnSessionConnected?.Invoke(currentSession);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            isCreatingSession = false;
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

    private async Task JoinSessionByID(string joinID)
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
        await JoinSessionByID(session.Id);
    }

    public async Task RejoinSession()
    {
        try
        {
            if (currentSession != null)
            {
                await currentSession.RefreshAsync();

                if (currentSession.HasPlayer(authenticationManager.PlayerID) == true)
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
        isLeavingSession = true;
        await currentSession.LeaveAsync();
        isLeavingSession = false;
        OnSessionEnded?.Invoke(currentSession);
        currentSession = null;
    }

    #endregion

    #region Kick Player

    public void KickPlayerByID(string playerID)
    {
        currentSession.AsHost().RemovePlayerAsync(playerID);
    }

    #endregion

    #region Update Session

    public async Task UpdateSessionData(bool isPrivate, string lobbyName)
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
        }
        catch (SessionException exception)
        {
            Debug.LogError(
                $"Falha ao atualizar sessão: {exception.Message}"
            );
        }
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
