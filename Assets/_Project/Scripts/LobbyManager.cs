using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public event Action<LobbyData> onLobbyUpdated;
    public event Action<PlayerData, NetworkListEvent<PlayerData>.EventType> onPlayersUpdated;

    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private LobbyNetworkState lobbyNetworkState;

    private FixedString64Bytes currentPlayerID;
    public SessionManager SessionManager => sessionManager;

    public void Start()
    {
        sessionManager.OnSessionCreated += SessionManagerOnSessionCreated;
        lobbyNetworkState.LobbyState.OnValueChanged += OnLobbyStateValueChanged;
        lobbyNetworkState.playersState.OnListChanged += PlayersStateOnOnListChanged;
        lobbyNetworkState.OnHostLeave += LobbyNetworkStateOnOnHostLeave;
    }

    private void LobbyNetworkStateOnOnHostLeave()
    {
        sessionManager.LeaveSession();
    }

    private void PlayersStateOnOnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        onPlayersUpdated?.Invoke(changeEvent.Value, changeEvent.Type);
    }

    private void OnLobbyStateValueChanged(LobbyData previousValue, LobbyData newValue)
    {
        onLobbyUpdated?.Invoke(newValue);
    }

    private void SessionManagerOnSessionCreated(ISession session)
    {
        currentPlayerID = AuthenticationService.Instance.PlayerId;
        sessionManager.CurrentSession.PlayerJoined += CurrentSessionOnPlayerJoined;
        sessionManager.CurrentSession.PlayerHasLeft += (id) => lobbyNetworkState.PlayerLeft(id);

        foreach (IReadOnlyPlayer readOnlyPlayer in session.Players)
        {
            PlayerData playerData = new PlayerData(readOnlyPlayer.Id, "");

            lobbyNetworkState.PlayerJoined(playerData);
        }

        lobbyNetworkState.CreateLobbyData(session.MaxPlayers, session.IsPrivate, session.Name, session.Code);

        if (lobbyNetworkState.IsServer == true || lobbyNetworkState.IsHost == true)
        {
            return;
        }

        onLobbyUpdated?.Invoke(lobbyNetworkState.LobbyState.Value);

        UpdatePlayersUsingNetworkState();
    }

    private void UpdatePlayersUsingNetworkState()
    {
        foreach (PlayerData playerData in lobbyNetworkState.PlayersState)
        {
            onPlayersUpdated?.Invoke(playerData, NetworkListEvent<PlayerData>.EventType.Add);
        }
    }

    private void CurrentSessionOnPlayerJoined(string id)
    {
        lobbyNetworkState.PlayerJoined(new PlayerData(id, ""));
    }

    public void ExitLobby(bool migrateHost)
    {
        lobbyNetworkState.CurrentPlayerLeave();
    }

    public void ToggleCurrentPlayerReady()
    {
        lobbyNetworkState.TogglePlayerReady(currentPlayerID);
    }

    public void CallUpdateLobby(bool isPrivate, string lobbyName)
    {
        lobbyNetworkState.UpdateLobbyData(isPrivate, lobbyName);
    }

    #region Getters

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

    #endregion
}
