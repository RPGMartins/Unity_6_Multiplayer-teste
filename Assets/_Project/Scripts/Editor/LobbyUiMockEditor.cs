using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LobbyUiMock))]
public class LobbyUiMockEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SessionManager sessionManager = ((LobbyUiMock)target).SessionManager;
        LobbyManager lobbyUIManager = ((LobbyUiMock)target).LobbyManager;

        if (sessionManager.CurrentSession != null)
        {
            if (sessionManager.CurrentSession.IsHost == true)
            {
                if (GUILayout.Button("Randomize Lobby Name"))
                {
                    sessionManager.UpdateSessionData(sessionManager.CurrentSession.IsPrivate, SessionUiMockEditor.GetRandomName());
                }

                if (GUILayout.Button("Start Match"))
                {
                }
            }

            if (GUILayout.Button("Toggle Ready"))
            {
                lobbyUIManager.ToggleCurrentPlayerReady();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Lobby Options");
            EditorGUILayout.LabelField($"Name: {lobbyUIManager.GetLobbyName()}");
            EditorGUILayout.LabelField($"Code: {lobbyUIManager.GetLobbyCode()}");
            EditorGUILayout.LabelField($"Host: {sessionManager.CurrentSession.Host}");

            foreach (PlayerData player in lobbyUIManager.GetLobbyPlayers())
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField($"PlayerID: {player.AuthenticationId}");
                EditorGUILayout.LabelField($"PlayerName: {player.Name}");
                EditorGUILayout.LabelField($"IsReady: {player.IsReady}");
            }
        }
    }
}
