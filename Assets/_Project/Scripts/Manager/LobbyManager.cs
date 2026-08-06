using System;
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
        OnSessionConnected += CreateLobbyAndPlayersData;
        OnSessionDisconnect += PlayerLeft;
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
        OnSessionConnected -= CreateLobbyAndPlayersData;
        OnSessionDisconnect -= PlayerLeft;
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

    private void RemoveClient(ulong clientID)
    {
        Debug.Log("ClientID: " + clientID);
        PlayerData playerData = lobbyNetworkState.GetPlayerDataByClientID(clientID);

        SessionUtility.RemovePlayerFromSession(currentSession.AsHost(), playerData.AuthenticationId.ToString(), OnStartTryRemovePlayerFromSession, OnRemovePlayerFromSession, OnRemovePlayerFromSessionFailed);

        if (clientID == 0) // is Host
        {
        }
        else
        {
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
        SessionUtility.ReconnectSession(currentSession, authenticationManager.PlayerID, OnSessionConnected, OnStartTryConnectToSession, OnSessionConnectFailed);
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

    private void CreateLobbyAndPlayersData(ISession session)
    {
        currentSession = session;
        session.PlayerJoined += CurrentSessionOnPlayerJoined;
        session.PlayerHasLeft += (id) => lobbyNetworkState.PlayerLeft(id, LeaveLobby);

        foreach (IReadOnlyPlayer readOnlyPlayer in session.Players)
        {
            PlayerData playerData = new PlayerData(readOnlyPlayer.Id, UInt64.MaxValue, authenticationManager.PlayerName);

            lobbyNetworkState.PlayerJoined(playerData);
        }

        lobbyNetworkState.CreateLobbyData(session.MaxPlayers, session.IsPrivate, session.Name, session.Code);
        lobbyNetworkState.UpdatePlayerIDRpc(authenticationManager.PlayerID);
    }

    private void PlayerLeft(ISession session)
    {
        session.PlayerJoined -= CurrentSessionOnPlayerJoined;

        currentSession = null;
    }

    private void CurrentSessionOnPlayerJoined(string id)
    {
        foreach (IReadOnlyPlayer readOnlyPlayer in currentSession.Players)
        {
            if (readOnlyPlayer.Id != id)
            {
                continue;
            }

            lobbyNetworkState.PlayerJoined(new PlayerData(id, UInt64.MaxValue, "GuestPlayer"));
            lobbyNetworkState.UpdatePlayerIDRpc(authenticationManager.PlayerID);

            break;
        }
    }

    public void ToggleCurrentPlayerReady()
    {
        lobbyNetworkState.TogglePlayerReady(authenticationManager.PlayerID);
    }

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
}
