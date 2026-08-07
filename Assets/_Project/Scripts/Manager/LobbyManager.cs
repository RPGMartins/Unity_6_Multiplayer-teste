using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public event Action<ISession> OnSessionConnected;
    public event Action<SessionError> OnSessionConnectFailed;
    public event Action<ISession> OnSessionDisconnect;
    public event Action<SessionError> OnSessionDisconnectFailed;
    public event Action<ISession> OnSessionUpdate;
    public event Action<SessionError> OnSessionUpdateFailed;
    public event Action<ISession> OnRemovePlayerFromSession;
    public event Action<SessionError> OnRemovePlayerFromSessionFailed;

    public event Action OnStartTryConnectToSession;
    public event Action OnStartTryDisconnectFromSession;
    public event Action OnStartTrySessionUpdate;
    public event Action OnStartTryRemovePlayerFromSession;

    public event Action<LobbyData> OnLobbyUpdated;
    public event Action<PlayerData, NetworkListEvent<PlayerData>.EventType> OnPlayersUpdated;

    [SerializeField] private AuthenticationManager authenticationManager;
    [SerializeField] private LobbyNetworkState lobbyNetworkState;

    private ISession currentSession;
    public ISession CurrentSession => currentSession;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        OnSessionConnected += PlayerJoinedSession;
        OnSessionDisconnect += PlayerLeftSession;
        OnSessionUpdate += lobbyNetworkState.UpdateSessionData;

        lobbyNetworkState.LobbyState.OnValueChanged += LobbyDataValueChanged;
        lobbyNetworkState.PlayersState.OnListChanged += LobbyPlayersDataChanged;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += RemoveClient;
    }

    private void OnDestroy()
    {
        OnSessionConnected -= PlayerJoinedSession;
        OnSessionDisconnect -= PlayerLeftSession;
        OnSessionUpdate -= lobbyNetworkState.UpdateSessionData;

        if (lobbyNetworkState != null)
        {
            lobbyNetworkState.LobbyState.OnValueChanged -= LobbyDataValueChanged;
            lobbyNetworkState.PlayersState.OnListChanged -= LobbyPlayersDataChanged;
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= RemoveClient;
        }
    }

    private void LobbyDataValueChanged(LobbyData previousValue, LobbyData newValue)
    {
        OnLobbyUpdated?.Invoke(newValue);
    }

    private void LobbyPlayersDataChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayersUpdated?.Invoke(changeEvent.Value, changeEvent.Type);
    }

    #region Session

    public void CreateSession(SessionOptions sessionOptions)
    {
        SessionUtility.CreateSessionAsHost(sessionOptions, OnSessionConnected, OnStartTryConnectToSession, OnSessionConnectFailed);
    }

    public void JoinSession(string joinCode)
    {
        SessionUtility.JoinSessionByCode(joinCode, OnSessionConnected, OnStartTryConnectToSession, OnSessionConnectFailed);
    }

    public void QuickJoinSession()
    {
        SessionUtility.QuickJoinLobby(OnSessionConnected, OnStartTryConnectToSession, OnSessionConnectFailed);
    }

    public void ReconnectSession()
    {
        SessionUtility.ReconnectSession(currentSession, authenticationManager.AuthenticationID, OnSessionConnected, OnStartTryConnectToSession, OnSessionConnectFailed);
    }

    public void LeaveLobby()
    {
        SessionUtility.LeaveSession(currentSession, OnSessionDisconnect, OnStartTryDisconnectFromSession, OnSessionDisconnectFailed);
    }

    public void UpdateLobbyData(bool isPrivate, string lobbyName)
    {
        SessionUtility.UpdateSessionData(currentSession, isPrivate, lobbyName, OnSessionUpdate, OnStartTrySessionUpdate, OnSessionUpdateFailed);
    }

    #endregion

    private void PlayerJoinedSession(ISession session)
    {
        currentSession = session;
        session.PlayerJoined += PlayerJoined;
        session.PlayerHasLeft += PlayerLeft;

        StartCoroutine(StartLobby(session));
    }

    private IEnumerator StartLobby(ISession session)
    {
        yield return new WaitUntil(() => lobbyNetworkState.IsSpawned == true);

        foreach (IReadOnlyPlayer readOnlyPlayer in session.Players)
        {
            PlayerData playerData = new PlayerData(readOnlyPlayer.Id, UInt64.MaxValue, authenticationManager.PlayerName);

            lobbyNetworkState.PlayerJoined(playerData);
        }

        lobbyNetworkState.CreateLobbyData(session.MaxPlayers, session.IsPrivate, session.Name, session.Code);
        lobbyNetworkState.UpdatePlayerIDRpc(authenticationManager.AuthenticationID);
    }

    private void PlayerLeftSession(ISession session)
    {
        session.PlayerJoined -= PlayerJoined;
        session.PlayerHasLeft -= PlayerLeft;

        currentSession = null;
    }

    private void PlayerJoined(string id)
    {
        foreach (IReadOnlyPlayer readOnlyPlayer in currentSession.Players)
        {
            if (readOnlyPlayer.Id != id)
            {
                continue;
            }

            lobbyNetworkState.PlayerJoined(new PlayerData(id, UInt64.MaxValue, "GuestPlayer"));
            lobbyNetworkState.UpdatePlayerIDRpc(authenticationManager.AuthenticationID);

            break;
        }
    }

    private void PlayerLeft(string authenticateID)
    {
        if (CurrentSession.IsHost == true)
        {
            lobbyNetworkState.PlayerLeft(authenticateID);

            if (authenticationManager.AuthenticationID == authenticateID)
            {
                LeaveLobby();
            }
        }
        else
        {
            if (authenticationManager.AuthenticationID == authenticateID)
            {
                LeaveLobby();
            }
        }
    }

    private void RemoveClient(ulong clientID)
    {
        PlayerData playerData = lobbyNetworkState.GetPlayerDataByClientID(clientID);

        if (playerData.AuthenticationID == authenticationManager.AuthenticationID)
        {
            LeaveLobby();
        }
        else if (CurrentSession.IsServer == true)
        {
            RemovePlayer(playerData.AuthenticationID.ToString());
        }
    }

    private void RemovePlayer(string authenticateID)
    {
        SessionUtility.RemovePlayerFromSession(currentSession.AsHost(), authenticateID, OnStartTryRemovePlayerFromSession, OnRemovePlayerFromSession, OnRemovePlayerFromSessionFailed);
    }

    #region LobbyMethods

    public void ToggleCurrentPlayerReady()
    {
        lobbyNetworkState.TogglePlayerReady(authenticationManager.AuthenticationID);
    }

    public void StartMatch()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Uttils

    public string GetLobbyName()
    {
        return lobbyNetworkState.LobbyState.Value.LobbyName.Value;
    }

    public string GetLobbyCode()
    {
        return lobbyNetworkState.LobbyState.Value.LobbyCode.Value;
    }

    public bool GetLobbyIsPrivate()
    {
        return lobbyNetworkState.LobbyState.Value.IsPrivate;
    }

    public List<PlayerData> GetLobbyPlayers()
    {
        return lobbyNetworkState.GetLobbyPlayers();
    }

    public string GetHostID()
    {
        return CurrentSession.Host;
    }

    #endregion
}
