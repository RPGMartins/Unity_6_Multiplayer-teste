using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public event Action<LobbyData> OnLobbyUpdated;
    public event Action<PlayerData, NetworkListEvent<PlayerData>.EventType> OnPlayersUpdated;

    [SerializeField] private AuthenticationManager authenticationManager;
    [SerializeField] private LobbyNetworkState lobbyNetworkState;

    private ISession currentSession;
    public ISession CurrentSession => currentSession;

    private void Awake()
    {
        SessionUtility.OnSessionConnected += CreateLobbyAndPlayersData;
        SessionUtility.OnSessionDisconnect += onSessionDisconnect;
        SessionUtility.OnSessionUpdate += lobbyNetworkState.UpdateSessionData;
        
        NetworkUtility.OnNetworkLostConnection += NetworkUtilityOnOnNetworkLostConnection;
        NetworkUtility.OnClientNetworkLostConnection += NetworkUtilityOnOnClientNetworkLostConnection;
        
        lobbyNetworkState.LobbyState.OnValueChanged += LobbyDataValueChanged;
        lobbyNetworkState.PlayersState.OnListChanged += LobbyPlayersDataChanged;
    }

    
    private void NetworkUtilityOnOnNetworkLostConnection(ulong a)
    {
        throw new NotImplementedException();
    }
    private void NetworkUtilityOnOnClientNetworkLostConnection(ulong clientID)
    {
        PlayerData playerData = lobbyNetworkState.GetPlayerDataByClientID(clientID);
        
        SessionUtility.KickClient(currentSession, playerData.AuthenticationId.ToString());
    }
    
    private void Start()
    {
        NetworkManager.Singleton.OnConnectionEvent += NetworkUtility.HandleConnection;
    }

    private void OnDestroy()
    {
        SessionUtility.OnSessionConnected -= CreateLobbyAndPlayersData;
        SessionUtility.OnSessionConnected -= onSessionDisconnect;
        SessionUtility.OnSessionUpdate -= lobbyNetworkState.UpdateSessionData;

        if (lobbyNetworkState != null)
        {
            lobbyNetworkState.LobbyState.OnValueChanged -= LobbyDataValueChanged;
            lobbyNetworkState.PlayersState.OnListChanged -= LobbyPlayersDataChanged;
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnConnectionEvent -= NetworkUtility.HandleConnection;
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
        SessionUtility.CreateSessionAsHost(sessionOptions);
    }

    public void JoinSession(string joinCode)
    {
        SessionUtility.JoinSessionByCode(joinCode);
    }

    public void QuickJoinSession()
    {
        SessionUtility.QuickJoinLobby();
    }

    public void ReconnectSession()
    {
        SessionUtility.ReconnectSession(currentSession, authenticationManager.PlayerID);
    }

    public void LeaveSession()
    {
        SessionUtility.LeaveSession(currentSession);
    }

    public void UpdateSessionData(bool isPrivate, string lobbyName)
    {
        SessionUtility.UpdateSessionData(currentSession, isPrivate, lobbyName);
    }

    #endregion

    private void CreateLobbyAndPlayersData(ISession session)
    {
        currentSession = session;
        session.PlayerJoined += CurrentSessionOnPlayerJoined;
        session.PlayerHasLeft += (id) => lobbyNetworkState.PlayerLeft(id, LeaveSession);

        foreach (IReadOnlyPlayer readOnlyPlayer in session.Players)
        {
            PlayerData playerData = new PlayerData(readOnlyPlayer.Id, UInt64.MaxValue, authenticationManager.PlayerName);

            lobbyNetworkState.PlayerJoined(playerData);
        }

        lobbyNetworkState.CreateLobbyData(session.MaxPlayers, session.IsPrivate, session.Name, session.Code);
        lobbyNetworkState.UpdatePlayerIDRpc(authenticationManager.PlayerID);
    }

    private void onSessionDisconnect(ISession session)
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

    public List<PlayerData> GetLobbyPlayers()
    {
        return lobbyNetworkState.GetLobbyPlayers();
    }
}
