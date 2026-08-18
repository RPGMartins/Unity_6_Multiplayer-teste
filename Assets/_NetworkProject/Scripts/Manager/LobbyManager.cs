using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    [SerializeField] private SceneControllerManager sceneControllerManager;

    private ISession currentSession;
    public ISession CurrentSession => currentSession;

    public void Initialize()
    {
        OnSessionConnected += PlayerJoinedSession;
        OnSessionDisconnect += PlayerLeftSession;
        OnSessionUpdate += lobbyNetworkState.UpdateSessionData;

        lobbyNetworkState.LobbyData.OnValueChanged += LobbyDataValueChanged;
        lobbyNetworkState.PlayersState.OnListChanged += LobbyPlayersDataChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback += RemoveClient;

        sceneControllerManager.Initiliaze(OnChangeScene, this);
    }

    private void OnDestroy()
    {
        OnSessionConnected -= PlayerJoinedSession;
        OnSessionDisconnect -= PlayerLeftSession;
        OnSessionUpdate -= lobbyNetworkState.UpdateSessionData;

        if (lobbyNetworkState != null)
        {
            lobbyNetworkState.LobbyData.OnValueChanged -= LobbyDataValueChanged;
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

    public async Task CreateSession(SessionOptions sessionOptions)
    {
        await SessionUtility.CreateSessionAsHost(sessionOptions, OnSessionConnected, OnStartTryConnectToSession, OnSessionConnectFailed);
    }

    public async Task JoinSession(string joinCode)
    {
        await SessionUtility.JoinSessionByCode(joinCode, OnSessionConnected, OnStartTryConnectToSession, OnSessionConnectFailed);
    }

    public async Task QuickJoinSession()
    {
        await SessionUtility.QuickJoinLobby(OnSessionConnected, OnStartTryConnectToSession, OnSessionConnectFailed);
    }

    public async Task ReconnectSession()
    {
        await SessionUtility.ReconnectSession(currentSession, authenticationManager.AuthenticationID.ToString(), OnSessionConnected, OnStartTryConnectToSession, OnSessionConnectFailed);
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
        lobbyNetworkState.UpdatePlayerIDRpc(authenticationManager.AuthenticationID.ToString());
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
            lobbyNetworkState.UpdatePlayerIDRpc(authenticationManager.AuthenticationID.ToString());

            break;
        }
    }

    private void PlayerLeft(string authenticateID) //Player left session //TODO olhar com mais certeza, se precisa de dois eventos de player saiu
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

    private void RemoveClient(ulong clientID) //Player lost conection
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

    private async Task RemovePlayer(string authenticateID)
    {
        await SessionUtility.RemovePlayerFromSession(currentSession.AsHost(), authenticateID, OnStartTryRemovePlayerFromSession, OnRemovePlayerFromSession, OnRemovePlayerFromSessionFailed);
    }

    #region LobbyMethods

    public void ToggleCurrentPlayerReady()
    {
        lobbyNetworkState.TogglePlayerReady(authenticationManager.AuthenticationID);
    }

    private void OnChangeScene(LobbyStateEnum lobbyStateEnum)
    {
        lobbyNetworkState.UpdateLobbyStateEnum(lobbyStateEnum);
    }

    #endregion

    #region Uttils

    public string GetLobbyName()
    {
        return lobbyNetworkState.LobbyData.Value.LobbyName.Value;
    }

    public string GetLobbyCode()
    {
        return lobbyNetworkState.LobbyData.Value.LobbyCode.Value;
    }

    public bool GetLobbyIsPrivate()
    {
        return lobbyNetworkState.LobbyData.Value.IsPrivate;
    }

    public List<PlayerData> GetLobbyPlayers()
    {
        return lobbyNetworkState.GetLobbyPlayers();
    }

    public string GetHostID()
    {
        return CurrentSession.Host;
    }

    public LobbyStateEnum GetLobbyState()
    {
        if (lobbyNetworkState.IsSpawned == true)
        {
            return lobbyNetworkState.LobbyStateEnum.Value;
        }

        return LobbyStateEnum.OnLobby;
    }

    #endregion
}
