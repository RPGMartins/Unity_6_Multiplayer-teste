using System.Collections.Generic;
using Unity.Services.Multiplayer;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private AuthenticationManager authenticationManager;
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private LobbyUIManager lobbyUIManager;
    [SerializeField] private LobbyNetworkState lobbyNetworkState;

    private void Awake()
    {
        sessionManager.OnSessionConnected += CreateLobbyAndPlayersData;
    }

    public void Start()
    {
        lobbyUIManager.Initialize(lobbyNetworkState.LobbyState, lobbyNetworkState.PlayersState);
    }

    private void CreateLobbyAndPlayersData(ISession session)
    {
        session.PlayerJoined += CurrentSessionOnPlayerJoined;
        session.PlayerHasLeft += (id) => lobbyNetworkState.PlayerLeft(id);

        foreach (IReadOnlyPlayer readOnlyPlayer in session.Players)
        {
            PlayerData playerData = new PlayerData(readOnlyPlayer.Id, authenticationManager.PlayerName);

            lobbyNetworkState.PlayerJoined(playerData);
        }

        lobbyNetworkState.CreateLobbyData(session.MaxPlayers, session.IsPrivate, session.Name, session.Code);
    }

    private void CurrentSessionOnPlayerJoined(string id)
    {
        foreach (IReadOnlyPlayer readOnlyPlayer in sessionManager.CurrentSession.Players)
        {
            if (readOnlyPlayer.Id != id)
            {
                continue;
            }

            lobbyNetworkState.PlayerJoined(new PlayerData(id, "GuestPlayer"));

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
