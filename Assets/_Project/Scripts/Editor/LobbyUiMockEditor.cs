using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LobbyUiMock))]
public class LobbyUiMockEditor : Editor
{
    private string[] serverNames = new string[] { "Ands Server", "Kekls server", "marrtinz server" };

    private int playersQuantity;
    private string lobbyName;
    private string passWord;
    private bool isPrivate;

    private string joinCode;
    private string playerName;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LobbyManager lobbyManager = ((LobbyUiMock)target).LobbyManager;
        SessionManager sessionManager = ((LobbyUiMock)target).SessionManager;
        GUIStyle textStyle = new GUIStyle();
        textStyle.normal.textColor = Color.white;
        EditorGUILayout.BeginVertical();
        playerName = EditorGUILayout.TextField("Player Name", playerName);

        if (GUILayout.Button("Update Name"))
        {
            AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
        }

        EditorGUILayout.EndVertical();

        if (sessionManager.CurrentSession == null)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Create Lobby Options");
            EditorGUILayout.Space(10);
            playersQuantity = EditorGUILayout.IntField("playersQuantity", playersQuantity);
            lobbyName = EditorGUILayout.TextField("LobbyName", lobbyName);
            passWord = EditorGUILayout.TextField("passWord", passWord);
            isPrivate = EditorGUILayout.Toggle("isPrivate", isPrivate);

            if (GUILayout.Button("Create Lobby"))
            {
                sessionManager.CreateSessionAsHost(CreateSessionOptions());
            }

            if (GUILayout.Button("Create Random Lobby"))
            {
                sessionManager.CreateSessionAsHost(CreateSessionOptions(serverNames[Random.Range(0, serverNames.Length)]));
            }

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Join Lobby Options");
            EditorGUILayout.Space(10);
            joinCode = EditorGUILayout.TextField("joinCode", joinCode);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Join Lobby"))
            {
                sessionManager.JoinSessionByCode(joinCode);
            }

            if (GUILayout.Button("Quick Join"))
            {
                sessionManager.QuickJoinLobby();
            }
        }
        else
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Lobby Options");
            EditorGUILayout.LabelField($"Name: {lobbyManager.GetLobbyName()}");
            EditorGUILayout.LabelField($"Code: {lobbyManager.GetLobbyCode()}");

            foreach (PlayerData player in lobbyManager.GetLobbyPlayers())
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField($"PlayerID: {player.AuthenticationId}");
                EditorGUILayout.LabelField($"PlayerName: {player.Name}");
                EditorGUILayout.LabelField($"IsReady: {player.IsReady}");
            }

            if (sessionManager.CurrentSession.IsHost == true)
            {
                if (GUILayout.Button("Randomize Lobby"))
                {
                    //lobbyManager.CallUpdateLobby(isPrivate, serverNames[Random.Range(0, serverNames.Length)]);
                }

                if (GUILayout.Button("Start Match"))
                {
                }
            }

            if (GUILayout.Button("Toggle Ready"))
            {
                //sessionManager.ToggleCurrentPlayerReady();
            }

            if (GUILayout.Button("Exit Lobby"))
            {
                //sessionManager.ExitLobby(false);
            }
        }
    }

    private SessionOptions CreateSessionOptions()
    {
        return new SessionOptions
        {
            MaxPlayers = playersQuantity,
            IsPrivate = isPrivate,
            Name = lobbyName,
            Password = string.IsNullOrWhiteSpace(passWord) ? null : passWord,
        }.WithRelayNetwork();
    }

    private SessionOptions CreateSessionOptions(string serverName)
    {
        return new SessionOptions
        {
            MaxPlayers = Random.Range(2, 8),
            IsPrivate = isPrivate,
            Name = serverName,
            Password = string.IsNullOrWhiteSpace(passWord) ? null : passWord,
        }.WithRelayNetwork();
    }
}
