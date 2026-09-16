using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using Random = UnityEngine.Random;

public static class SessionUtility
{
    private static bool isCreatingSession;
    private static bool isLeavingSession;

    #region Create Session

    public static async Task CreateSessionAsHost(SessionOptions options, Action<ISession> onSessionConnected, Action onStartTryConnectToSession, Action<SessionError> onSessionConnectFailed)
    {
        onStartTryConnectToSession?.Invoke();

        if (CanCreateSession() == false)
        {
            onSessionConnectFailed?.Invoke(SessionError.None);

            return;
        }

        isCreatingSession = true;

        try
        {
            bool oldIsLocked = options.IsLocked;
            options.IsLocked = true;
            IHostSession currentSession = await MultiplayerService.Instance.CreateSessionAsync(options);
            currentSession.IsLocked = oldIsLocked;
            await currentSession.SavePropertiesAsync();
            onSessionConnected?.Invoke(currentSession);
        }
        catch (SessionException exception)
        {
            onSessionConnectFailed?.Invoke(exception.Error);
        }
        catch (Exception e)
        {
            onSessionConnectFailed?.Invoke(SessionError.None);
        }
        finally
        {
            isCreatingSession = false;
        }
    }

    #endregion

    #region Join Session

    public static async Task JoinSessionByCode(string joinCode, Action<ISession> onSessionConnected, Action onStartTryConnectToSession, Action<SessionError> onSessionConnectFailed)
    {
        onStartTryConnectToSession?.Invoke();

        try
        {
            JoinSessionOptions joinSession = new JoinSessionOptions
            {
                Type = "Lobby"
            };

            ISession currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode, joinSession);
            onSessionConnected?.Invoke(currentSession);
        }
        catch (SessionException exception)
        {
            onSessionConnectFailed?.Invoke(exception.Error);
        }
        catch (Exception e)
        {
            onSessionConnectFailed?.Invoke(SessionError.None);
        }
    }

    public static async Task JoinSessionByID(string joinID, Action<ISession> onSessionConnected, Action onStartTryConnectToSession, Action<SessionError> onSessionConnectFailed)
    {
        onStartTryConnectToSession?.Invoke();

        try
        {
            JoinSessionOptions joinSession = new JoinSessionOptions
            {
                Type = "Lobby"
            };

            ISession currentSession = await MultiplayerService.Instance.JoinSessionByIdAsync(joinID, joinSession);
            onSessionConnected?.Invoke(currentSession);
        }
        catch (SessionException exception)
        {
            onSessionConnectFailed?.Invoke(exception.Error);
        }
        catch (Exception e)
        {
            onSessionConnectFailed?.Invoke(SessionError.None);
        }
    }

    public static async Task QuickJoinLobby(Action<ISession> onSessionConnected, Action onStartTryConnectToSession, Action<SessionError> onSessionConnectFailed)
    {
        QuerySessionsOptions querySessionsOptions = new QuerySessionsOptions();
        QuerySessionsResults querySessionsResults = await MultiplayerService.Instance.QuerySessionsAsync(querySessionsOptions);

        ISessionInfo session = querySessionsResults.Sessions[Random.Range(0, querySessionsResults.Sessions.Count)];
        await JoinSessionByID(session.Id, onSessionConnected, onStartTryConnectToSession, onSessionConnectFailed);
    }

    public static async Task ReconnectSession(ISession currentSession, string playerID, Action<ISession> onSessionConnected, Action onStartTryConnectToSession, Action<SessionError> onSessionConnectFailed)
    {
        onStartTryConnectToSession?.Invoke();

        if (currentSession != null)
        {
            await currentSession.RefreshAsync();

            if (currentSession.HasPlayer(playerID) == true)
            {
                try
                {
                    await currentSession.ReconnectAsync();
                    NetworkManager.Singleton.StartClient();
                    onSessionConnected?.Invoke(currentSession);
                }
                catch (SessionException exception)
                {
                    onSessionConnectFailed?.Invoke(exception.Error);
                }
                catch (Exception e)
                {
                    onSessionConnectFailed?.Invoke(SessionError.None);
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
            onSessionConnected?.Invoke(currentSession);
        }
        catch (SessionException exception)
        {
            onSessionConnectFailed?.Invoke(exception.Error);
        }
        catch (Exception e)
        {
            onSessionConnectFailed?.Invoke(SessionError.None);
        }
    }

    #endregion

    #region Leave Session

    public static async Task LeaveSession(ISession currentSession, Action<ISession> onSessionDisconnect, Action onStartTryDisconnectFromSession, Action<SessionError> onSessionDisconnectFailed)
    {
        if (isLeavingSession == true)
        {
            return;
        }

        onStartTryDisconnectFromSession?.Invoke();
        isLeavingSession = true;

        try
        {
            await currentSession.LeaveAsync();
            onSessionDisconnect?.Invoke(currentSession);
        }
        catch (SessionException sessionException)
        {
            onSessionDisconnectFailed?.Invoke(sessionException.Error);
        }
        catch (Exception e)
        {
            onSessionDisconnectFailed?.Invoke(SessionError.None);
        }
        finally
        {
            isLeavingSession = false;
        }
    }

    #endregion

    #region Update Session

    public static async Task UpdateSessionData(ISession currentSession, bool isPrivate, string lobbyName, Action<ISession> onSessionUpdate, Action onStartTrySessionUpdate, Action<SessionError> onSessionUpdateFailed)
    {
        if (currentSession.IsHost == false)
        {
            return;
        }

        onStartTrySessionUpdate?.Invoke();

        try
        {
            IHostSession hostSession = currentSession.AsHost();

            hostSession.Name = lobbyName;
            hostSession.IsPrivate = isPrivate;

            await hostSession.SavePropertiesAsync();
            onSessionUpdate?.Invoke(currentSession);
        }
        catch (SessionException sessionException)
        {
            onSessionUpdateFailed?.Invoke(sessionException.Error);
        }
        catch (Exception e)
        {
            onSessionUpdateFailed?.Invoke(SessionError.None);
        }
    }

    #endregion

    #region Kick Session

    public static async Task RemovePlayerFromSession(ISession currentSession, string authenticateID, Action onStartTryRemovePlayerFromSession, Action<ISession> onRemovePlayerFromSession, Action<SessionError> onRemovePlayerFromSessionFailed)
    {
        if (currentSession.IsServer == false)
        {
            return;
        }

        onStartTryRemovePlayerFromSession?.Invoke();

        try
        {
            await currentSession.AsHost().RemovePlayerAsync(authenticateID);
            onRemovePlayerFromSession?.Invoke(currentSession);
        }
        catch (SessionException sessionException)
        {
            onRemovePlayerFromSessionFailed?.Invoke(sessionException.Error);
        }
        catch (Exception e)
        {
            onRemovePlayerFromSessionFailed?.Invoke(SessionError.None);
        }
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
