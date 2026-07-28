using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using Random = UnityEngine.Random;

public class LobbyNetworkState : NetworkBehaviour
{
    public event Action OnHostLeave;

    private NetworkVariable<LobbyData> lobbyState = new NetworkVariable<LobbyData>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkList<PlayerData> playersState;
    public NetworkVariable<LobbyData> LobbyState => lobbyState;
    public NetworkList<PlayerData> PlayersState => playersState;

    private SessionManager sessionManager;

    public override void OnNetworkSpawn()
    {
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer == true && IsHost == true)
        {
            ResetVariables();
        }
        else
        {
            OnHostLeave.Invoke();
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
        sessionManager = FindAnyObjectByType<SessionManager>();
        playersState = new NetworkList<PlayerData>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }

    public void CreateLobbyData(int maxPlayers, bool isPrivate, string lobbyName, string lobbyCode)
    {
        if (IsServer == false)
        {
            return;
        }

        UpdateLobbyData(new LobbyData(maxPlayers, isPrivate, lobbyName, lobbyCode));
    }

    public async void UpdateLobbyData(bool isPrivate, string lobbyName)
    {
        if (IsServer == false && IsHost == false)
        {
            return;
        }

        LobbyData newLobby = new LobbyData(lobbyState.Value.MaxPlayers, isPrivate, lobbyName, lobbyState.Value.LobbyCode);
        await UpdateSessionAsync(sessionManager.CurrentSession, newLobby);
        UpdateLobbyData(newLobby);
    }

    private async Task UpdateSessionAsync(ISession session, LobbyData lobbyData)
    {
        try
        {
            IHostSession hostSession = session.AsHost();

            hostSession.Name = lobbyData.LobbyName.Value;
            hostSession.IsPrivate = lobbyData.IsPrivate;

            await hostSession.SavePropertiesAsync();

            Debug.Log(
                $"Sessão atualizada: " +
                $"{hostSession.Name}, " +
                $"privada: {hostSession.IsPrivate}"
            );
        }
        catch (SessionException exception)
        {
            Debug.LogError(
                $"Falha ao atualizar sessão: {exception.Message}"
            );
        }
    }

    private void UpdateLobbyData(LobbyData lobbyData)
    {
        lobbyState.Value = lobbyData;
    }

    public void PlayerJoined(PlayerData playerData)
    {
        if (IsServer == false)
        {
            return;
        }

        playersState.Add(new PlayerData(playerData));
    }

    public void PlayerLeft(string playerID)
    {
        int playerIndex = GetPlayerDataIndex(playerID);
        playersState.RemoveAt(playerIndex);
    }

    public void CurrentPlayerLeave()
    {
        if (IsHost == true && IsServer == true)
        {
            //sessionManager.LeaveSessionAsClient();
        }

        sessionManager.LeaveSessionAsClient(); //TODO colocar migração de host
    }

    public void TogglePlayerReady(FixedString64Bytes id)
    {
        if (IsServer == false)
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

    [Rpc(SendTo.Server)]
    private void TogglePlayerReadyRpc(FixedString64Bytes id)
    {
        TogglePlayerReady(id);
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
        lobbyState = new NetworkVariable<LobbyData>();
        playersState.Clear();
    }
}
