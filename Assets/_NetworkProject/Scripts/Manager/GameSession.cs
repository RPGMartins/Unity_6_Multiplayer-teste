using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Services.Multiplayer;
using UnityEngine;

public class GameSession
{
    public event Action<string> PlayerJoined;
    public event Action<string> PlayerHasLeft;

    private SessionData sessionData;
    private ISession onlineSession;
    private List<PlayerData> currentPlayersOnSession = new List<PlayerData>();

    public SessionData SessionData => sessionData;
    private bool IsOnline => onlineSession != null;
    public List<PlayerData> CurrentPlayersOnSession => currentPlayersOnSession;

    public GameSession(ISession onlineSession)
    {
        this.onlineSession = onlineSession;

        onlineSession.PlayerJoined += PlayerJoined;
        onlineSession.PlayerHasLeft += PlayerHasLeft;
    }

    public GameSession(SessionData sessionData, ISession onlineSession) : this(onlineSession)
    {
        this.sessionData = sessionData;
    }

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
        if (IsOnline == true)
        {
            await onlineSession.LeaveAsync();

            return;
        }

        //TODO Sair da sessão namoral
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

    public async Task UpdateSessionData(string lobbyName, bool isPrivate)
    {
        if (IsHost() == false)
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

        sessionData = new SessionData(sessionData.MaxPlayers, isPrivate, lobbyName, sessionData.LobbyCode);
    }

    #region UTILS

    public bool HasPlayer(FixedString64Bytes playerID)
    {
        foreach (PlayerData playerData in currentPlayersOnSession)
        {
            if (playerData.AuthenticationID.Value == playerID.Value)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsHost()
    {
        if (IsOnline == false)
        {
            return true;
        }

        return onlineSession.IsHost;
    }

    public bool IsServer()
    {
        if (IsOnline == false)
        {
            return true;
        }

        return onlineSession.IsServer;
    }

    #endregion
}
