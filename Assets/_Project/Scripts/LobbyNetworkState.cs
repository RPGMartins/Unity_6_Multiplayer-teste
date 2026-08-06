using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

public class LobbyNetworkState : NetworkBehaviour
{
    private NetworkVariable<LobbyData> lobbyState = new NetworkVariable<LobbyData>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkList<PlayerData> playersState;

    public NetworkVariable<LobbyData> LobbyState => lobbyState;
    public NetworkList<PlayerData> PlayersState => playersState;

    public override void OnNetworkSpawn()
    {
        ResetVariables();
    }

    public override void OnNetworkDespawn()
    {
        ResetVariables();
    }

    private void Awake()
    {
        playersState = new NetworkList<PlayerData>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }

    #region Lobby

    public void CreateLobbyData(int maxPlayers, bool isPrivate, string lobbyName, string lobbyCode)
    {
        if (IsServer == false)
        {
            return;
        }

        lobbyState.Value = new LobbyData(maxPlayers, isPrivate, lobbyName, lobbyCode);
    }

    public void UpdateSessionData(ISession session)
    {
        CreateLobbyData(session.MaxPlayers, session.IsPrivate, session.Name, session.Code);
    }

    #endregion

    #region Player Join/Leave

    public void PlayerJoined(PlayerData playerData)
    {
        if (IsServer == false)
        {
            return;
        }

        playersState.Add(new PlayerData(playerData));
    }

    public void PlayerLeft(string playerID, Action leftCallback)
    {
        if (IsServer == false)
        {
            leftCallback.Invoke();

            return;
        }

        int playerIndex = GetPlayerIndexByAuthenticateID(playerID);
        playersState.RemoveAt(playerIndex);
    }

    #endregion

    #region RPC

    [Rpc(SendTo.Server)]
    private void TogglePlayerReadyRpc(FixedString64Bytes id)
    {
        TogglePlayerReady(id);
    }

    [Rpc(SendTo.Server)]
    public void UpdatePlayerIDRpc(string playerID, RpcParams rpcParams = default)
    {
        if (IsServer == false)
        {
            return;
        }

        ulong clientId = rpcParams.Receive.SenderClientId;
        int index = GetPlayerIndexByAuthenticateID(playerID);
        PlayerData tempPlayerData = playersState[index];
        tempPlayerData.SetClientID(clientId);
        playersState[index] = tempPlayerData;
    }

    #endregion

    public void TogglePlayerReady(FixedString64Bytes id)
    {
        if (IsServer == false)
        {
            TogglePlayerReadyRpc(id);

            return;
        }

        int playerIndex = GetPlayerIndexByAuthenticateID(id);

        if (playerIndex >= 0)
        {
            PlayerData player = playersState[playerIndex];
            player.ToggleReady();
            playersState[playerIndex] = player;
        }
    }

    public int GetPlayerIndexByAuthenticateID(FixedString64Bytes id)
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

    public int GetPlayerIndexByClientID(ulong clientID)
    {
        for (int i = 0; i < playersState.Count; i++)
        {
            if (playersState[i].ClientId.Equals(clientID))
            {
                return i;
            }
        }

        return -1;
    }

    public PlayerData GetPlayerDataByClientID(ulong clientID)
    {
        int index = GetPlayerIndexByClientID(clientID);

        return playersState[index];
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

    private void ResetVariables()
    {
        if (IsServer == true)
        {
            lobbyState.Value = new LobbyData();
            playersState.Clear();
        }
    }
}
