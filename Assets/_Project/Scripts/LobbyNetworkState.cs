using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

public class LobbyNetworkState : NetworkBehaviour
{
    public event Action OnNetworkSpawnCallback;
    public event Action OnHostLeave;

    private NetworkVariable<LobbyData> lobbyState = new NetworkVariable<LobbyData>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkList<PlayerData> playersState;
    public NetworkVariable<LobbyData> LobbyState => lobbyState;
    public NetworkList<PlayerData> PlayersState => playersState;

    private SessionManager sessionManager;

    public override void OnNetworkSpawn()
    {
        OnNetworkSpawnCallback?.Invoke();

        if (IsHost == true)
        {
            ResetVariables();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsHost == true)
        {
            ResetVariables();
        }
    }

    public List<PlayerData> GetLobbyPlayers()
    {
        List<PlayerData> playersToReturn = new List<PlayerData>();

        foreach (PlayerData player in PlayersState)
        {
            playersToReturn.Add(player);
        }

        return playersToReturn;
    }

    private void Awake()
    {
        playersState = new NetworkList<PlayerData>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        sessionManager = FindAnyObjectByType<SessionManager>();
        sessionManager.OnSessionUpdate += UpdateLobbyData;
        sessionManager.OnSessionHostUpdated += PlayerHostUpdate;
    }

    #region Lobby

    public void CreateLobbyData(int maxPlayers, bool isPrivate, string lobbyName, string lobbyCode)
    {
        if (IsHost == false)
        {
            return;
        }

        UpdateLobbyData(new LobbyData(maxPlayers, isPrivate, lobbyName, lobbyCode));
    }

    private void UpdateLobbyData(LobbyData lobbyData)
    {
        lobbyState.Value = lobbyData;
    }

    #endregion

    #region Player Join/Leave

    public void PlayerJoined(PlayerData playerData)
    {
        if (IsHost == false)
        {
            return;
        }

        playersState.Add(new PlayerData(playerData));
    }

    public void PlayerLeft(string playerID)
    {
        if (IsHost == false)
        {
            sessionManager.LeaveSessionAsClient();

            return;
        }

        int playerIndex = GetPlayerDataIndex(playerID);
        playersState.RemoveAt(playerIndex);
    }

    public void PlayerHostUpdate(string playerID)
    {
        if (IsHost == false)
        {
            return;
        }
    }

    #endregion

    public void TogglePlayerReady(FixedString64Bytes id)
    {
        if (IsHost == false)
        {
            TogglePlayerReadyRpc(id);

            return;
        }

        int playerIndex = GetPlayerDataIndex(id);

        if (playerIndex >= 0)
        {
            PlayerData player = playersState[playerIndex];
            player.ToggleReady();
            playersState[playerIndex] = player;
        }
    }

    private int GetPlayerDataIndex(FixedString64Bytes id)
    {
        for (int i = 0; i < playersState.Count; i++)
        {
            if (playersState[i].AuthenticationId.Equals(id))
            {
                return i;
            }
        }

        return -1;
    }

    private void ResetVariables()
    {
        lobbyState.Value = new LobbyData();
        playersState.Clear();
    }

    [Rpc(SendTo.Server)]
    private void TogglePlayerReadyRpc(FixedString64Bytes id)
    {
        TogglePlayerReady(id);
    }
}
