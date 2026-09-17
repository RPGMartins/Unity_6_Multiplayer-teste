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

    public void Draw(SessionManager sessionManager, NetworkSimulator networkSimulator, SceneControllerManager sceneControllerManager)
    {
        if (networkSimulator != null)
        {
            DrawNetworkSimulator(networkSimulator);
            GUILayout.Space(20);
        }

        if (sessionManager != null)
        {
            if (sessionManager.GetLobbyState() == LobbyStateEnum.OnLobby)
            {
                DrawCreateLobby(sessionManager);
                DrawJoinLobby(sessionManager);
                GUILayout.Space(20);
            }

            DrawLobbyOptions(sessionManager, sceneControllerManager);
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

    private void DrawCreateLobby(SessionManager sessionManager)
    {
        if (sessionManager.CurrentSession != null)
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
            sessionManager.CreateSession(options);
        }

        if (GUILayout.Button("Create Random Lobby"))
        {
            SessionOptions options = CreateSessionOptions().WithRelayNetwork();
            sessionManager.CreateSession(options);
        }
    }

    private void DrawJoinLobby(SessionManager sessionManager)
    {
        if (sessionManager.CurrentSession != null)
        {
            return;
        }

        GUILayout.Space(15);
        GUILayout.Label("Join Lobby Options");

        DrawTextField("Código da sala:", ref joinCode);

        if (GUILayout.Button("Join Lobby"))
        {
            sessionManager.JoinSessionByCode(joinCode);
        }

        if (GUILayout.Button("Quick Join"))
        {
            sessionManager.QuickJoinSession();
        }

        if (GUILayout.Button("Rejoin Lobby"))
        {
            sessionManager.ReconnectSession();
        }
    }

    private async void DrawLobbyOptions(SessionManager sessionManager, SceneControllerManager sceneControllerManager)
    {
        if (sessionManager.CurrentSession == null)
        {
            return;
        }

        if (sessionManager.CurrentSession.OnlineSession.IsServer == true)
        {
            DrawTextField("Nome do Lobby:", ref lobbyName);
            isPrivate = GUILayout.Toggle(isPrivate, "Sala Privada");

            if (GUILayout.Button("Update Lobby"))
            {
                sessionManager.UpdateLobbyData(isPrivate, lobbyName);
            }

            if (GUILayout.Button("Update Lobby Random"))
            {
                sessionManager.UpdateLobbyData(isPrivate, GetRandomLobbyName());
            }
        }

        GUILayout.Label("Lobby Options");
        GUILayout.Label($"Name: {sessionManager.GetLobbyName()}");
        GUILayout.Label($"Code: {sessionManager.GetLobbyCode()}");
        GUILayout.Label($"Host: {sessionManager.GetHostID()}");
        GUILayout.Label($"Privado: {sessionManager.GetLobbyIsPrivate()}");

        foreach (PlayerData player in sessionManager.GetLobbyPlayers())
        {
            GUILayout.Space(10);
            GUILayout.Label($"AuthenticateID: {player.AuthenticationID}");
            GUILayout.Label($"ClientID: {player.ClientId}");
            GUILayout.Label($"PlayerName: {player.Name}");
            GUILayout.Label($"IsReady: {player.IsReady}");
        }

        if (sessionManager.GetLobbyState() == LobbyStateEnum.OnLobby)
        {
            if (sessionManager.CurrentSession.OnlineSession.IsHost == true)
            {
                if (GUILayout.Button("Start Match"))
                {
                    sceneControllerManager.GotoGameplayScene();
                }
            }

            if (GUILayout.Button("Toggle Ready"))
            {
                sessionManager.ToggleCurrentPlayerReady();
            }
        }
        else if (sessionManager.GetLobbyState() == LobbyStateEnum.OnGameplay)
        {
            if (sessionManager.CurrentSession.OnlineSession.IsHost == true)
            {
                if (GUILayout.Button("Back To Lobby"))
                {
                    sceneControllerManager.GotoMainMenuScene();
                }
            }
        }

        if (GUILayout.Button("Leave Lobby"))
        {
            await sessionManager.LeaveLobby();

            if (sessionManager.GetLobbyState() == LobbyStateEnum.OnGameplay)
            {
                sceneControllerManager.GotoMainMenuScene();
            }
        }
    }

    private static void DrawTextField(string label, ref string value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(130));
        value = GUILayout.TextField(value ?? string.Empty);
        GUILayout.EndHorizontal();
    }

    public static SessionOptions CreateSessionOptions(string name = "", bool isPrivate = false, int maxPlayers = -1, string password = "")
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
