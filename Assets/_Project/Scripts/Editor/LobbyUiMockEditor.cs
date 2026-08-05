using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LobbyUiMock))]
public class LobbyUiMockEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        LobbyManager lobbyManager = ((LobbyUiMock)target).LobbyManager;

        if (lobbyManager.CurrentSession != null)
        {
            if (lobbyManager.CurrentSession.IsHost == true)
            {
                if (GUILayout.Button("Randomize Lobby Name"))
                {
                    lobbyManager.UpdateSessionData(lobbyManager.CurrentSession.IsPrivate, SessionUiMockEditor.GetRandomName());
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
            EditorGUILayout.LabelField($"Host: {lobbyManager.CurrentSession.Host}");

            foreach (PlayerData player in lobbyManager.GetLobbyPlayers())
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField($"AuthenticateID: {player.AuthenticationId}");
                EditorGUILayout.LabelField($"PlayerID: {player.ClientId}");
                EditorGUILayout.LabelField($"PlayerName: {player.Name}");
                EditorGUILayout.LabelField($"IsReady: {player.IsReady}");
            }
        }
    }
}
