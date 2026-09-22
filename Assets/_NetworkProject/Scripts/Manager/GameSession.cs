using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

public class GameSession : NetworkBehaviour
{
    public event Action<PlayerData> PlayerHasJoined;
    public event Action<PlayerData> PlayerHasLeft;
    public event Action<SessionData> OnSessionUpdated;
    public event Action<PlayerData, NetworkListEvent<PlayerData>.EventType> OnPlayersUpdated;

    [SerializeField] private AuthenticationManager authenticationManager;
    [SerializeField] private SessionData sessionData;
    [SerializeField] private List<PlayerData> currentPlayersOnSession = new List<PlayerData>();

    private ISession onlineSession;
    private SessionStateEnum sessionStateEnum = SessionStateEnum.OnLobby;

    private NetworkVariable<SessionData> networkSessionData = new NetworkVariable<SessionData>();
    private NetworkList<PlayerData> networkCurrentPlayersOnSession;
    private NetworkVariable<SessionStateEnum> networkSessionStateEnum = new NetworkVariable<SessionStateEnum>();

    public SessionData SessionData => sessionData;
    private bool IsOnline => onlineSession != null;
    public List<PlayerData> CurrentPlayersOnSession => currentPlayersOnSession;

    private void Awake()
    {
        networkCurrentPlayersOnSession = new NetworkList<PlayerData>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer == true)
        {
            networkSessionData.Value = new SessionData();
            networkCurrentPlayersOnSession.Clear();
            networkSessionStateEnum.Value = SessionStateEnum.OnLobby;
        }

        networkSessionData.OnValueChanged += SessionDataValueChanged;
        networkCurrentPlayersOnSession.OnListChanged += HandleSessionPlayersDataChanged;
    }

    public override void OnNetworkDespawn()
    {
        networkSessionData.OnValueChanged -= SessionDataValueChanged;
        networkCurrentPlayersOnSession.OnListChanged -= HandleSessionPlayersDataChanged;
    }

    public void Initialize(ISession session, SessionData sessionData, Action<GameSession> onSessionInitialized)
    {
        ResetLocalVariables();
        StartCoroutine(WaitNetworkBehaviorSpawn(AfterSpawnCallback));

        void AfterSpawnCallback()
        {
            this.onlineSession = session;
            onlineSession.PlayerJoined += PlayerJoined;
            onlineSession.PlayerHasLeft += PlayerLeft;

            if (HasHostAuthority() == false)
            {
                SetSessionData(networkSessionData.Value);

                foreach (PlayerData playerData in networkCurrentPlayersOnSession)
                {
                    currentPlayersOnSession.Add(playerData);
                    PlayerJoined(playerData);
                }
            }
            else
            {
                SetSessionData(sessionData);

                foreach (IReadOnlyPlayer readOnlyPlayer in session.Players)
                {
                    PlayerData playerData = new PlayerData(readOnlyPlayer.Id, NetworkManager.LocalClientId, authenticationManager.PlayerName, true); //TODO ver se existe possibildade de isso nem sempre ser true
                    PlayerJoined(playerData);
                }

                UpdatePlayerIDRpc();
            }

            onSessionInitialized.Invoke(this);
        }
    }

    private IEnumerator WaitNetworkBehaviorSpawn(Action callback)
    {
        yield return new WaitUntil(() => IsSpawned == true);

        callback.Invoke();
    }

    [Rpc(SendTo.Server)]
    private void UpdatePlayerIDRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        bool hasPlayer = GetPlayerDataByClientID(clientId.ToString(), out PlayerData playerData);

        if (hasPlayer == false)
        {
            return;
        }

        playerData.SetClientID(clientId);
        UpdatePlayerData(playerData);
    }

    #region PlayerJoin

    private void PlayerJoined(string clientId)
    {
        PlayerData playerData = CreateGuestPlayer(clientId);
        PlayerJoined(playerData);
    }

    private void PlayerJoined(PlayerData playerData)
    {
        if (HasHostAuthority() == false)
        {
            return;
        }

        if (IsOnline == true)
        {
            foreach (PlayerData tempPlayerData in networkCurrentPlayersOnSession)
            {
                if (tempPlayerData.AuthenticationID == playerData.AuthenticationID)
                {
                    return;
                }
            }

            networkCurrentPlayersOnSession.Add(playerData);

            return;
        }

        foreach (PlayerData tempPlayerData in currentPlayersOnSession)
        {
            if (tempPlayerData.AuthenticationID == playerData.AuthenticationID)
            {
                return;
            }
        }

        AddPlayerLocalToList(playerData);
    }

    private void PlayerJoinedCallback(PlayerData playerData)
    {
        PlayerHasJoined?.Invoke(playerData);
    }

    private PlayerData CreateGuestPlayer(FixedString64Bytes guestPlayerID)
    {
        return new PlayerData(guestPlayerID, UInt64.MaxValue, "Guest", false); //TODO pegar esse nome de algum lugar
    }

    #endregion

    #region PlayerLeft

    private void PlayerLeft(string playerID)
    {
        bool hasPlayer = GetPlayerDataByPlayerID(playerID, out PlayerData playerData);

        if (hasPlayer == false)
        {
            return;
        }

        PlayerLeft(playerData);
    }

    private void PlayerLeft(PlayerData playerData)
    {
        if (HasHostAuthority() == false)
        {
            if (playerData.IsHost == true)
            {
                LeaveSessionAsync();
            }

            return;
        }

        if (IsOnline == true)
        {
            networkCurrentPlayersOnSession.Remove(playerData);
        }
        else
        {
            RemovePlayerLocalToList(playerData);
        }
    }

    private void PlayerLeftCallback(PlayerData playerData)
    {
        PlayerHasLeft?.Invoke(playerData);
    }

    #endregion

    public async Task RefreshAsync()
    {
        if (IsOnline == false)
        {
            Debug.LogError("Chamando Refresh enquanto está no modo offline");

            return;
        }

        await onlineSession.RefreshAsync();
    }

    public async Task ReconnectAsync()
    {
        if (IsOnline == false)
        {
            Debug.LogError("Chamando Reconnect enquanto está no modo offline");

            return;
        }

        await onlineSession.ReconnectAsync();
    }

    public async Task LeaveSessionAsync()
    {
        ResetLocalVariables();

        if (IsOnline == true)
        {
            await onlineSession.LeaveAsync();
        }
        else
        {
            //Saida normal
        }
    }

    public async Task RemovePlayerAsync(FixedString64Bytes playerID)
    {
        if (IsOnline == true)
        {
            await onlineSession.AsHost().RemovePlayerAsync(playerID.Value);
        }

        for (int i = currentPlayersOnSession.Count - 1; i >= 0; i--)
        {
            if (currentPlayersOnSession[i].AuthenticationID.Value != playerID.Value)
            {
                continue;
            }

            currentPlayersOnSession.RemoveAt(i);

            break;
        }
    }

    #region Ready

    public void ToggleCurrentPlayerReady()
    {
        TogglePlayerReady(authenticationManager.AuthenticationID);
    }

    private void TogglePlayerReady(FixedString64Bytes playerID)
    {
        if (HasHostAuthority() == false)
        {
            TogglePlayerReadyRpc(playerID);

            return;
        }

        bool hasPlayer = GetPlayerDataByPlayerID(playerID, out PlayerData playerData);

        if (hasPlayer == false)
        {
            throw new Exception($"Player {playerID.Value} not found");
        }

        playerData.ToggleReady();
        UpdatePlayerData(playerData);
    }

    [Rpc(SendTo.Server)]
    private void TogglePlayerReadyRpc(FixedString64Bytes playerID)
    {
        TogglePlayerReady(playerID);
    }

    #endregion

    #region SessionData

    public async Task UpdateSessionData(string lobbyName, bool isPrivate)
    {
        if (HasHostAuthority() == false)
        {
            return;
        }

        if (IsOnline == true)
        {
            IHostSession hostSession = onlineSession.AsHost();
            hostSession.Name = lobbyName;
            hostSession.IsPrivate = isPrivate;
            await hostSession.SavePropertiesAsync();
        }

        SetSessionData(new SessionData(sessionData.MaxPlayers, isPrivate, lobbyName, sessionData.LobbyCode));
    }

    private void SetSessionData(SessionData sessionData)
    {
        if (HasHostAuthority() == false)
        {
            this.sessionData = sessionData;

            return;
        }

        if (IsOnline == true)
        {
            networkSessionData.Value = sessionData;
        }

        this.sessionData = sessionData;
    }

    private void SessionDataValueChanged(SessionData previousValue, SessionData newValue)
    {
        OnSessionUpdated?.Invoke(newValue);
    }

    #endregion

    #region PlayerData

    private void HandleSessionPlayersDataChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<PlayerData>.EventType.Add:
                AddPlayerLocalToList(changeEvent.Value);

                break;

            case NetworkListEvent<PlayerData>.EventType.Remove:
                RemovePlayerLocalToList(changeEvent.Value);

                break;

            case NetworkListEvent<PlayerData>.EventType.Value:
                UpdateLocalPlayerData(changeEvent.Value);

                break;

            case NetworkListEvent<PlayerData>.EventType.Clear:
                ClearPlayerLocalList();

                break;
        }

        OnPlayersUpdated?.Invoke(changeEvent.Value, changeEvent.Type);
    }

    private void AddPlayerLocalToList(PlayerData playerData)
    {
        currentPlayersOnSession.Add(playerData);
        PlayerJoinedCallback(playerData);
    }

    private void RemovePlayerLocalToList(PlayerData playerData)
    {
        currentPlayersOnSession.Remove(playerData);
        PlayerLeftCallback(playerData);
    }

    private void ClearPlayerLocalList()
    {
        currentPlayersOnSession.Clear();
    }

    private void UpdatePlayerData(PlayerData playerData)
    {
        if (IsOnline == true)
        {
            UpdateNetworkPlayerData(playerData);
        }
        else
        {
            UpdateLocalPlayerData(playerData);
        }
    }

    private void UpdateNetworkPlayerData(PlayerData playerData)
    {
        for (int i = 0; i < networkCurrentPlayersOnSession.Count; i++)
        {
            if (networkCurrentPlayersOnSession[i].AuthenticationID.Value == playerData.AuthenticationID)
            {
                networkCurrentPlayersOnSession[i] = playerData;

                break;
            }
        }
    }

    private void UpdateLocalPlayerData(PlayerData playerData)
    {
        for (int i = 0; i < currentPlayersOnSession.Count; i++)
        {
            if (currentPlayersOnSession[i].AuthenticationID.Value == playerData.AuthenticationID)
            {
                currentPlayersOnSession[i] = playerData;
            }
        }
    }

    #endregion

    #region UTILS

    public bool HasHostAuthority()
    {
        if (IsOnline == false)
        {
            return true;
        }

        return onlineSession.IsHost;
    }

    public bool GetPlayerDataByClientID(string clientID, out PlayerData returnPlayerData)
    {
        if (IsOnline == true)
        {
            foreach (PlayerData playerData in networkCurrentPlayersOnSession)
            {
                if (playerData.ClientId.ToString() != clientID)
                {
                    continue;
                }

                returnPlayerData = playerData;

                return true;
            }
        }
        else
        {
            foreach (PlayerData playerData in currentPlayersOnSession)
            {
                if (playerData.ClientId.ToString() != clientID)
                {
                    continue;
                }

                returnPlayerData = playerData;

                return true;
            }
        }

        returnPlayerData = default;

        return false;
    }

    public bool GetPlayerDataByPlayerID(FixedString64Bytes playerID, out PlayerData returnPlayerData)
    {
        if (IsOnline == true)
        {
            foreach (PlayerData playerData in networkCurrentPlayersOnSession)
            {
                if (playerData.AuthenticationID != playerID)
                {
                    continue;
                }

                returnPlayerData = playerData;

                return true;
            }
        }
        else
        {
            foreach (PlayerData playerData in currentPlayersOnSession)
            {
                if (playerData.AuthenticationID != playerID)
                {
                    continue;
                }

                returnPlayerData = playerData;

                return true;
            }
        }

        returnPlayerData = default;

        return false;
    }

    private void ResetLocalVariables()
    {
        currentPlayersOnSession.Clear();
        sessionData = default;
        sessionStateEnum = SessionStateEnum.OnLobby;
    }

    #endregion
}
