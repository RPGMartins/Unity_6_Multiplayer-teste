using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LobbyUiMock))]
public class LobbyUiMockEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SessionManager sessionManager = ((LobbyUiMock)target).SessionManager;
        LobbyManager lobbyManager = ((LobbyUiMock)target).LobbyManager;

        if (sessionManager.CurrentSession != null)
        {
            if (sessionManager.CurrentSession.IsHost == true)
            {
                if (GUILayout.Button("Randomize Lobby Name"))
                {
                    sessionManager.UpdateLobbyData(sessionManager.CurrentSession.IsPrivate, SessionUiMockEditor.GetRandomName());
                }

                if (GUILayout.Button("Start Match"))
                {
                }
            }

            if (GUILayout.Button("Toggle Ready"))
            {
                lobbyManager.ToggleCurrentPlayerReady();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Lobby Options");
            EditorGUILayout.LabelField($"Name: {lobbyManager.GetLobbyName()}");
            EditorGUILayout.LabelField($"Code: {lobbyManager.GetLobbyCode()}");
            EditorGUILayout.LabelField($"Host: {lobbyManager.SessionManager.CurrentSession.Host}");

            foreach (PlayerData player in lobbyManager.GetLobbyPlayers())
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField($"PlayerID: {player.AuthenticationId}");
                EditorGUILayout.LabelField($"PlayerName: {player.Name}");
                EditorGUILayout.LabelField($"IsReady: {player.IsReady}");
            }
        }
    }
}
