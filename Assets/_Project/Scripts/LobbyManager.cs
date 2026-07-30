using System;
using System.Collections.Generic;
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

    private IAuthenticationService currentPlayer;

    public SessionManager SessionManager => sessionManager;

    private void Awake()
    {
        sessionManager.OnAuthenticatedSucceed += SessionManagerOnOnAuthenticatedSucceed;
        sessionManager.OnSessionConnected += SessionManagerOnSessionConnected;
    }

    public void Start()
    {
        lobbyNetworkState.LobbyState.OnValueChanged += OnLobbyStateValueChanged;
        lobbyNetworkState.playersState.OnListChanged += PlayersStateOnOnListChanged;
    }

    private void SessionManagerOnOnAuthenticatedSucceed()
    {
        currentPlayer = AuthenticationService.Instance;
    }

    private void PlayersStateOnOnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        onPlayersUpdated?.Invoke(changeEvent.Value, changeEvent.Type);
    }

    private void OnLobbyStateValueChanged(LobbyData previousValue, LobbyData newValue)
    {
        onLobbyUpdated?.Invoke(newValue);
    }

    private void SessionManagerOnSessionConnected(ISession session)
    {
        session.PlayerJoined += CurrentSessionOnPlayerJoined;
        session.PlayerHasLeft += (id) => lobbyNetworkState.PlayerLeft(id);

        foreach (IReadOnlyPlayer readOnlyPlayer in session.Players)
        {
            PlayerData playerData = new PlayerData(readOnlyPlayer.Id, currentPlayer.PlayerName);

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
        foreach (IReadOnlyPlayer readOnlyPlayer in sessionManager.CurrentSession.Players)
        {
            if (readOnlyPlayer.Id == id)
            {
                lobbyNetworkState.PlayerJoined(new PlayerData(id, "GuestPlayer"));

                break;
            }
        }
    }

    public void ToggleCurrentPlayerReady()
    {
        lobbyNetworkState.TogglePlayerReady(currentPlayer.PlayerId);
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
