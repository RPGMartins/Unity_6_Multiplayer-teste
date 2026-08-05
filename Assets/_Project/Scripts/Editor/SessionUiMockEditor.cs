using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CustomEditor(typeof(SessionUiMock))]
public class SessionUiMockEditor : Editor
{
    private static string[] serverNames = new string[] { "Ands Server", "Kekls server", "marrtinz server" };

    private int playersQuantity;
    private string lobbyName;
    private string passWord;
    private bool isPrivate;

    private string joinCode;
    private string playerName;

    private LobbyManager lobbyManager;

    private void Awake()
    {
        lobbyManager = ((SessionUiMock)target).LobbyManager;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        base.OnInspectorGUI();

        GUIStyle textStyle = new GUIStyle();
        textStyle.normal.textColor = Color.white;
        EditorGUILayout.BeginVertical();
        playerName = EditorGUILayout.TextField("Player Name", playerName);

        if (GUILayout.Button("Update Name"))
        {
            AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
        }

        EditorGUILayout.EndVertical();

        if (lobbyManager.CurrentSession == null)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Create Session Options");
            EditorGUILayout.Space(10);
            playersQuantity = EditorGUILayout.IntField("playersQuantity", playersQuantity);
            lobbyName = EditorGUILayout.TextField("LobbyName", lobbyName);
            passWord = EditorGUILayout.TextField("passWord", passWord);
            isPrivate = EditorGUILayout.Toggle("isPrivate", isPrivate);

            if (GUILayout.Button("Create Session"))
            {
                lobbyManager.CreateSession(CreateSessionOptions());
            }

            if (GUILayout.Button("Create Random Session"))
            {
                lobbyManager.CreateSession(CreateSessionOptions(GetRandomName()));
            }

            if (GUILayout.Button("Update Session"))
            {
                lobbyManager.UpdateSessionData(isPrivate, GetRandomName());
            }

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Join Lobby Options");
            EditorGUILayout.Space(10);
            joinCode = EditorGUILayout.TextField("joinCode", joinCode);
            EditorGUILayout.EndVertical();

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
        else
        {
            if (GUILayout.Button("Exit Session"))
            {
                lobbyManager.LeaveSession();
            }
        }
    }

    private SessionOptions CreateSessionOptions()
    {
        return new SessionOptions
        {
            Type = "Lobby",
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
            Type = "Lobby",
            MaxPlayers = Random.Range(2, 8),
            IsPrivate = isPrivate,
            Name = serverName,
            Password = string.IsNullOrWhiteSpace(passWord) ? null : passWord,
        }.WithRelayNetwork();
    }

    public static string GetRandomName()
    {
        return serverNames[Random.Range(0, serverNames.Length)];
    }
}
