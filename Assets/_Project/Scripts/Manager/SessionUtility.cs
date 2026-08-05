using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using Random = UnityEngine.Random;

public static class SessionUtility
{
    public static event Action<ISession> OnSessionConnected;
    public static event Action<SessionError> OnSessionConnectFailed;
    public static event Action<ISession> OnSessionDisconnect;
    public static event Action<SessionError> OnSessionDisconnectFailed;

    public static event Action OnStartTryConnectToSession;
    public static event Action OnStartTryDisconnectFromSession;

    public static event Action OnSessionLostConnection;
    public static event Action<ISession> OnSessionUpdate;

    private static bool isCreatingSession;

    #region Create Session

    public static async Task CreateSessionAsHost(SessionOptions options)
    {
        OnStartTryConnectToSession?.Invoke();

        if (CanCreateSession() == false)
        {
            OnSessionConnectFailed?.Invoke(SessionError.None);

            return;
        }

        isCreatingSession = true;

        try
        {
            IHostSession currentSession = await MultiplayerService.Instance.CreateSessionAsync(options);
            OnSessionConnected?.Invoke(currentSession);
        }
        catch (SessionException exception)
        {
            OnSessionConnectFailed?.Invoke(exception.Error);
        }
        catch (Exception e)
        {
            OnSessionConnectFailed?.Invoke(SessionError.None);
        }
        finally
        {
            isCreatingSession = false;
        }
    }

    #endregion

    #region Join Session

    public static async Task JoinSessionByCode(string joinCode)
    {
        OnStartTryConnectToSession?.Invoke();

        try
        {
            JoinSessionOptions joinSession = new JoinSessionOptions
            {
                Type = "Lobby"
            };

            ISession currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode, joinSession);
            OnSessionConnected?.Invoke(currentSession);
        }
        catch (SessionException exception)
        {
            OnSessionConnectFailed?.Invoke(exception.Error);
        }
        catch (Exception e)
        {
            OnSessionConnectFailed?.Invoke(SessionError.None);
        }
    }

    private static async Task JoinSessionByID(string joinID)
    {
        OnStartTryConnectToSession?.Invoke();

        try
        {
            JoinSessionOptions joinSession = new JoinSessionOptions
            {
                Type = "Lobby"
            };

            ISession currentSession = await MultiplayerService.Instance.JoinSessionByIdAsync(joinID, joinSession);
            OnSessionConnected?.Invoke(currentSession);
        }
        catch (SessionException exception)
        {
            OnSessionConnectFailed?.Invoke(exception.Error);
        }
        catch (Exception e)
        {
            OnSessionConnectFailed?.Invoke(SessionError.None);
        }
    }

    public static async Task QuickJoinLobby()
    {
        QuerySessionsOptions querySessionsOptions = new QuerySessionsOptions();
        QuerySessionsResults querySessionsResults = await MultiplayerService.Instance.QuerySessionsAsync(querySessionsOptions);

        ISessionInfo session = querySessionsResults.Sessions[Random.Range(0, querySessionsResults.Sessions.Count)];
        await JoinSessionByID(session.Id);
    }

    public static async Task ReconnectSession(ISession currentSession, string playerID)
    {
        OnStartTryConnectToSession?.Invoke();

        if (currentSession != null)
        {
            await currentSession.RefreshAsync();

            if (currentSession.HasPlayer(playerID) == true)
            {
                try
                {
                    await currentSession.ReconnectAsync();
                    NetworkManager.Singleton.StartClient();
                    OnSessionConnected?.Invoke(currentSession);
                }
                catch (SessionException exception)
                {
                    OnSessionConnectFailed?.Invoke(exception.Error);
                }
                catch (Exception e)
                {
                    OnSessionConnectFailed?.Invoke(SessionError.None);
                }

                return;
            }
        }

        List<string> sessionIds = await MultiplayerService.Instance.GetJoinedSessionIdsAsync();

        if (sessionIds == null || sessionIds.Count == 0)
        {
            return;
        }

        try
        {
            string sessionId = sessionIds[0];
            currentSession = await MultiplayerService.Instance.ReconnectToSessionAsync(sessionId);
            NetworkManager.Singleton.StartClient();
            OnSessionConnected?.Invoke(currentSession);
        }
        catch (SessionException exception)
        {
            OnSessionConnectFailed?.Invoke(exception.Error);
        }
        catch (Exception e)
        {
            OnSessionConnectFailed?.Invoke(SessionError.None);
        }
    }

    #endregion

    #region Leave Session

    public static async Task LeaveSession(ISession currentSession)
    {
        OnStartTryDisconnectFromSession?.Invoke();

        try
        {
            await currentSession.LeaveAsync();
            OnSessionDisconnect?.Invoke(currentSession);
        }
        catch (SessionException sessionException)
        {
            OnSessionDisconnectFailed?.Invoke(sessionException.Error);
        }
        catch (Exception e)
        {
            OnSessionDisconnectFailed?.Invoke(SessionError.None);
        }
    }

    #endregion

    #region Update Session

    public static async Task UpdateSessionData(ISession currentSession, bool isPrivate, string lobbyName)
    {
        if (currentSession.IsHost == false)
        {
            return;
        }

        await UpdateSessionAsync(currentSession, isPrivate, lobbyName);
        OnSessionUpdate?.Invoke(currentSession);
    }

    private static async Task UpdateSessionAsync(ISession currentSession, bool isPrivate, string lobbyName)
    {
        try
        {
            IHostSession hostSession = currentSession.AsHost();

            hostSession.Name = lobbyName;
            hostSession.IsPrivate = isPrivate;

            await hostSession.SavePropertiesAsync();
        }
        catch (SessionException e)
        {
        }
    }

    #endregion

    #region Kick Session

    public static void KickClient(ISession currentSession, string clientAuthenticateID)
    {
        currentSession.AsHost().RemovePlayerAsync(clientAuthenticateID);
    }

    #endregion
    private static bool CanCreateSession()
    {
        if (isCreatingSession == true)
        {
            Debug.LogWarning("A sessionUI is already being created.");

            return false;
        }

        return true;
    }
}
