using Unity.Multiplayer.Tools.NetworkSimulator.Runtime;
using Unity.Services.Multiplayer;
using UnityEngine;

public class MultiplayerDebugDrawer
{
    private static readonly string[] serverNames = new[] { "Ands Server", "Kkesls Server", " Matrins Server" };

    private string lobbyName = string.Empty;
    private string playerQuantityText = "4";
    private string password = string.Empty;
    private string joinCode = string.Empty;
    private bool isPrivate;

    public void Draw(LobbyManager lobbyManager, NetworkSimulator networkSimulator)
    {
        if (networkSimulator != null)
        {
            DrawNetworkSimulator(networkSimulator);
            GUILayout.Space(20);
        }

        if (lobbyManager != null)
        {
            DrawCreateLobby(lobbyManager);
            DrawJoinLobby(lobbyManager);
            GUILayout.Space(20);
            DrawLobbyOptions(lobbyManager);
        }
    }

    private void DrawNetworkSimulator(NetworkSimulator networkSimulator)
    {
        GUILayout.Label("Network Simulator");
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Connected"))
        {
            networkSimulator.Reconnect();
        }

        if (GUILayout.Button("Disconnected"))
        {
            networkSimulator.Disconnect();
        }

        GUILayout.EndHorizontal();
    }

    private void DrawCreateLobby(LobbyManager lobbyManager)
    {
        if (lobbyManager.CurrentSession != null)
        {
            return;
        }

        GUILayout.Label("Create Lobby Options");

        DrawTextField("Nome do Lobby:", ref lobbyName);
        DrawTextField("Quantidade:", ref playerQuantityText);
        DrawTextField("Senha:", ref password);
        isPrivate = GUILayout.Toggle(isPrivate, "Sala Privada");

        if (GUILayout.Button("Create Lobby"))
        {
            int.TryParse(playerQuantityText, out int playerQuantity);

            SessionOptions options = CreateSessionOptions(lobbyName, isPrivate, playerQuantity, password).WithRelayNetwork();
            lobbyManager.CreateSession(options);
        }

        if (GUILayout.Button("Create Random Lobby"))
        {
            SessionOptions options = CreateSessionOptions().WithRelayNetwork();
            lobbyManager.CreateSession(options);
        }
    }

    private void DrawJoinLobby(LobbyManager lobbyManager)
    {
        if (lobbyManager.CurrentSession != null)
        {
            return;
        }

        GUILayout.Space(15);
        GUILayout.Label("Join Lobby Options");

        DrawTextField("Código da sala:", ref joinCode);

        if (GUILayout.Button("Join Session"))
        {
            lobbyManager.JoinSession(joinCode);
        }

        if (GUILayout.Button("Quick Join"))
        {
            lobbyManager.QuickJoinSession();
        }

        if (GUILayout.Button("Rejoin Session"))
        {
            lobbyManager.ReconnectSession();
        }
    }

    private void DrawLobbyOptions(LobbyManager lobbyManager)
    {
        if (lobbyManager.CurrentSession == null)
        {
            return;
        }

        if (lobbyManager.CurrentSession.IsServer == true)
        {
            DrawTextField("Nome do Lobby:", ref lobbyName);
            isPrivate = GUILayout.Toggle(isPrivate, "Sala Privada");

            if (GUILayout.Button("Update Lobby"))
            {
                lobbyManager.UpdateLobbyData(isPrivate, lobbyName);
            }

            if (GUILayout.Button("Update Lobby Random"))
            {
                lobbyManager.UpdateLobbyData(isPrivate, GetRandomLobbyName());
            }
        }

        GUILayout.Label("Lobby Options");
        GUILayout.Label($"Name: {lobbyManager.GetLobbyName()}");
        GUILayout.Label($"Code: {lobbyManager.GetLobbyCode()}");
        GUILayout.Label($"Host: {lobbyManager.CurrentSession.Host}");
        GUILayout.Label($"Privado: {lobbyManager.GetLobbyIsPrivate()}");

        foreach (PlayerData player in lobbyManager.GetLobbyPlayers())
        {
            GUILayout.Space(10);
            GUILayout.Label($"AuthenticateID: {player.AuthenticationId}");
            GUILayout.Label($"ClientID: {player.ClientId}");
            GUILayout.Label($"PlayerName: {player.Name}");
            GUILayout.Label($"IsReady: {player.IsReady}");
        }

        if (GUILayout.Button("Toggle Ready"))
        {
            lobbyManager.ToggleCurrentPlayerReady();
        }

        if (GUILayout.Button("Exit Session"))
        {
            lobbyManager.LeaveLobby();
        }
    }

    private static void DrawTextField(string label, ref string value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(130));
        value = GUILayout.TextField(value ?? string.Empty);
        GUILayout.EndHorizontal();
    }

    private static SessionOptions CreateSessionOptions(string name = "", bool isPrivate = false, int maxPlayers = -1, string password = "")
    {
        if (name == string.Empty)
        {
            name = GetRandomLobbyName();
        }

        if (maxPlayers == -1)
        {
            maxPlayers = Random.Range(3, 8);
        }

        SessionOptions options = new SessionOptions
        {
            Type = "Lobby",
            Name = name,
            MaxPlayers = maxPlayers,
            IsPrivate = isPrivate,
            Password = string.IsNullOrWhiteSpace(password) ? null : password
        };

        return options;
    }

    private static string GetRandomLobbyName()
    {
        return serverNames[Random.Range(0, serverNames.Length)];
    }
}
