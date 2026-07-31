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

    private ISession sessionToUse;
    private SessionManager sessionManager;

    private void Awake()
    {
        sessionManager = ((SessionUiMock)target).SessionManager;
        sessionManager.OnSessionConnected += SessionManagerOnOnSessionConnected;
        sessionManager.OnSessionLostConnection += SessionManagerOnOnSessionLostConnection;
        sessionManager.OnSessionEnded += SessionManagerOnOnSessionLostConnection;
    }

    private void SessionManagerOnOnSessionConnected(ISession session)
    {
        sessionToUse = session;
    }

    private void SessionManagerOnOnSessionLostConnection(ISession session)
    {
        sessionToUse = null;
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

        if (sessionToUse == null)
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
                sessionManager.CreateSessionAsHost(CreateSessionOptions());
            }

            if (GUILayout.Button("Create Random Session"))
            {
                sessionManager.CreateSessionAsHost(CreateSessionOptions(GetRandomName()));
            }

            if (GUILayout.Button("Update Session"))
            {
                sessionManager.CreateSessionAsHost(CreateSessionOptions(GetRandomName()));
            }

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Join Lobby Options");
            EditorGUILayout.Space(10);
            joinCode = EditorGUILayout.TextField("joinCode", joinCode);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Join Session"))
            {
                sessionManager.JoinSessionByCode(joinCode);
            }

            if (GUILayout.Button("Quick Join"))
            {
                sessionManager.QuickJoinLobby();
            }

            if (GUILayout.Button("Rejoin Session"))
            {
                sessionManager.RejoinSession();
            }
        }
        else
        {
            if (GUILayout.Button("Exit Session"))
            {
                sessionManager.LeaveSession();
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

    public static string GetRandomName()
    {
        return serverNames[Random.Range(0, serverNames.Length)];
    }
}
