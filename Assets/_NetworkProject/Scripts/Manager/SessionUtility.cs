using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using Random = UnityEngine.Random;

public static class SessionUtility
{
    private static bool isCreatingSession;
    private static bool isLeavingSession;

    #region Create Session

    public static async Task CreateSessionAsHost(SessionOptions options, Action<GameSession> onSessionConnected, Action onStartTryConnectToSession, Action<SessionError> onSessionConnectFailed)
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
            IHostSession currentSession = null;
            SessionData sessionData = new SessionData(1, true, "Singleplayer", default);

            if (options != null)
            {
                bool oldIsLocked = options.IsLocked;
                options.IsLocked = true;
                currentSession = await MultiplayerService.Instance.CreateSessionAsync(options);
                currentSession.IsLocked = oldIsLocked;
                await currentSession.SavePropertiesAsync();
                sessionData = new SessionData(currentSession.MaxPlayers, currentSession.IsPrivate, currentSession.Name, currentSession.Code);
            }

            GameSession gameSession = new GameSession(sessionData, currentSession);
            onSessionConnected?.Invoke(gameSession);
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

    public static async Task JoinSessionByCode(string joinCode, Action<GameSession> onSessionConnected, Action onStartTryConnectToSession, Action<SessionError> onSessionConnectFailed)
    {
        onStartTryConnectToSession?.Invoke();

        try
        {
            JoinSessionOptions joinSession = new JoinSessionOptions
            {
                Type = "Lobby"
            };

            ISession currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode, joinSession);
            GameSession gameSession = new GameSession(currentSession);

            onSessionConnected?.Invoke(gameSession);
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

    public static async Task JoinSessionByID(string joinID, Action<GameSession> onSessionConnected, Action onStartTryConnectToSession, Action<SessionError> onSessionConnectFailed)
    {
        onStartTryConnectToSession?.Invoke();

        try
        {
            JoinSessionOptions joinSession = new JoinSessionOptions
            {
                Type = "Lobby"
            };

            ISession currentSession = await MultiplayerService.Instance.JoinSessionByIdAsync(joinID, joinSession);
            GameSession gameSession = new GameSession(currentSession);

            onSessionConnected?.Invoke(gameSession);
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

    public static async Task QuickJoinLobby(Action<GameSession> onSessionConnected, Action onStartTryConnectToSession, Action<SessionError> onSessionConnectFailed)
    {
        QuerySessionsOptions querySessionsOptions = new QuerySessionsOptions();
        QuerySessionsResults querySessionsResults = await MultiplayerService.Instance.QuerySessionsAsync(querySessionsOptions);

        ISessionInfo session = querySessionsResults.Sessions[Random.Range(0, querySessionsResults.Sessions.Count)];
        await JoinSessionByID(session.Id, onSessionConnected, onStartTryConnectToSession, onSessionConnectFailed);
    }

    public static async Task ReconnectSession(GameSession currentSession, FixedString64Bytes playerID, Action<GameSession> onSessionConnected, Action onStartTryConnectToSession, Action<SessionError> onSessionConnectFailed)
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
            ISession reconnectToSessionAsync = await MultiplayerService.Instance.ReconnectToSessionAsync(sessionId);
            GameSession gameSession = new GameSession(reconnectToSessionAsync);
            NetworkManager.Singleton.StartClient();
            onSessionConnected?.Invoke(gameSession);
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

    public static async Task LeaveSession(GameSession currentSession, Action<GameSession> onSessionDisconnect, Action onStartTryDisconnectFromSession, Action<SessionError> onSessionDisconnectFailed)
    {
        if (isLeavingSession == true)
        {
            return;
        }

        onStartTryDisconnectFromSession?.Invoke();
        isLeavingSession = true;

        try
        {
            await currentSession.LeaveSessionAsync();
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

    public static async Task UpdateSessionData(GameSession currentSession, bool isPrivate, string lobbyName, Action<GameSession> onSessionUpdate, Action onStartTrySessionUpdate, Action<SessionError> onSessionUpdateFailed)
    {
        if (currentSession.IsHost() == false)
        {
            return;
        }

        onStartTrySessionUpdate?.Invoke();

        try
        {
            await currentSession.UpdateSessionData(lobbyName, isPrivate);
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

    public static async Task RemovePlayerFromSession(GameSession currentSession, string authenticateID, Action onStartTryRemovePlayerFromSession, Action<GameSession> onRemovePlayerFromSession, Action<SessionError> onRemovePlayerFromSessionFailed)
    {
        if (currentSession.IsServer() == false)
        {
            return;
        }

        onStartTryRemovePlayerFromSession?.Invoke();

        try
        {
            await currentSession.RemovePlayerAsync(authenticateID);
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
