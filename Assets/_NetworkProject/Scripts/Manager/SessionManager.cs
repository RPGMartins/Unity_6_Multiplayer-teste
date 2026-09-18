using System;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

public class SessionManager : NetworkBehaviour
{
    public event Action<GameSession> OnSessionConnected;
    public event Action<SessionError> OnSessionConnectFailed;
    public event Action<GameSession> OnSessionDisconnect;
    public event Action<SessionError> OnSessionDisconnectFailed;
    public event Action<GameSession> OnSessionUpdate;
    public event Action<SessionError> OnSessionUpdateFailed;
    public event Action<GameSession> OnRemovePlayerFromSession;
    public event Action<SessionError> OnRemovePlayerFromSessionFailed;

    public event Action OnStartTryConnectToSession;
    public event Action OnStartTryDisconnectFromSession;
    public event Action OnStartTrySessionUpdate;
    public event Action OnStartTryRemovePlayerFromSession;

    [SerializeField] private AuthenticationManager authenticationManager;
    [SerializeField] private SceneControllerManager sceneControllerManager;
    [SerializeField] private GameSession currentSession;
    public GameSession CurrentSession => currentSession;

    public static SessionManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        OnSessionConnected -= PlayerJoinedSession;
        OnSessionDisconnect -= PlayerLeftSession;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    public void Initialize()
    {
        OnSessionConnected += PlayerJoinedSession;
        OnSessionDisconnect += PlayerLeftSession;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    #region Session

    public async Task CreateSession(SessionOptions sessionOptions)
    {
        await SessionUtility.CreateSessionAsHost(currentSession, sessionOptions, OnSessionConnected, OnStartTryConnectToSession, OnSessionConnectFailed);
    }

    public async Task JoinSessionByCode(string joinCode)
    {
        await SessionUtility.JoinSessionByCode(joinCode, currentSession, OnSessionConnected, OnStartTryConnectToSession, OnSessionConnectFailed);
    }

    public async Task JoinSessionByID(string sessionID)
    {
        await SessionUtility.JoinSessionByID(sessionID, currentSession, OnSessionConnected, OnStartTryConnectToSession, OnSessionConnectFailed);
    }

    public async Task QuickJoinSession()
    {
        await SessionUtility.QuickJoinLobby(currentSession, OnSessionConnected, OnStartTryConnectToSession, OnSessionConnectFailed);
    }

    public async Task ReconnectSession()
    {
        await SessionUtility.ReconnectSession(currentSession, authenticationManager.AuthenticationID, OnSessionConnected, OnStartTryConnectToSession, OnSessionConnectFailed);
    }

    public async Task LeaveLobby()
    {
        await SessionUtility.LeaveSession(currentSession, OnSessionDisconnect, OnStartTryDisconnectFromSession, OnSessionDisconnectFailed);
    }

    public async Task UpdateLobbyData(bool isPrivate, string lobbyName)
    {
        await SessionUtility.UpdateSessionData(currentSession, isPrivate, lobbyName, OnSessionUpdate, OnStartTrySessionUpdate, OnSessionUpdateFailed);
    }

    #endregion

    private void PlayerJoinedSession(GameSession session)
    {
        currentSession = session;
        session.PlayerLeftaaa += PlayerLeftaaa;
    }

    private void PlayerLeftSession(GameSession session)
    {
        session.PlayerLeftaaa -= PlayerLeftaaa;
        currentSession = null;
    }

    private void PlayerLeftaaa(string playerID)
    {
        if (authenticationManager.AuthenticationID == playerID)
        {
            LeaveLobby();
        }
    }

    private void OnClientDisconnected(ulong clientID)
    {
        bool hasPlayer = currentSession.GetPlayerDataByClientID(clientID, out PlayerData playerData);

        if (hasPlayer == false)
        {
            return;
        }

        if (playerData.AuthenticationID == authenticationManager.AuthenticationID)
        {
            LeaveLobby();
        }
        else if (CurrentSession.HasHostAuthority() == true)
        {
            RemovePlayerFromSession(playerData.AuthenticationID);
        }
    }

    private async Task RemovePlayerFromSession(FixedString64Bytes playerID)
    {
        await SessionUtility.RemovePlayerFromSession(currentSession, playerID, OnStartTryRemovePlayerFromSession, OnRemovePlayerFromSession, OnRemovePlayerFromSessionFailed);
    }
}
