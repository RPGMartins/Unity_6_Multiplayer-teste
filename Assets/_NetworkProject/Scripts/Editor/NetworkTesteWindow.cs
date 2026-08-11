using Unity.Multiplayer.Tools.NetworkSimulator.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class NetworkTesteWindow : EditorWindow
{
    private LobbyManager lobbyManager;
    private NetworkSimulator networkSimulator;
    private SceneControllerManager sceneControllerManager;

    private MultiplayerDebugDrawer multiplayerDebugDrawer;

    private VisualElement lobbyTab;
    private ScrollView networkSimulatorTab;

    private ToolbarToggle lobbyTabButton;
    private ToolbarToggle networkSimulatorTabButton;

    [MenuItem("Tools/MP TESTE")]
    private static void OpenWindow()
    {
        NetworkTesteWindow window = GetWindow<NetworkTesteWindow>();
        window.titleContent = new GUIContent("MP TESTE");
        window.minSize = new Vector2(450f, 500f);
        window.Show();
    }

    private void CreateGUI()
    {
        lobbyManager = FindFirstObjectByType<LobbyManager>(FindObjectsInactive.Include);
        networkSimulator = FindFirstObjectByType<NetworkSimulator>(FindObjectsInactive.Include);
        sceneControllerManager = FindFirstObjectByType<SceneControllerManager>(FindObjectsInactive.Include);

        if (lobbyManager == null || networkSimulator == null)
        {
            Debug.LogWarning("LobbyManager ou NetworkSimulator nao foram encontrados.");

            return;
        }

        if (multiplayerDebugDrawer == null)
        {
            multiplayerDebugDrawer = new MultiplayerDebugDrawer();
        }

        rootVisualElement.Clear();
        rootVisualElement.style.flexDirection = FlexDirection.Column;

        CreateToolbar();
        CreateLobbyTab();
        CreateNetworkSimulatorTab();

        rootVisualElement.Add(lobbyTab);
        rootVisualElement.Add(networkSimulatorTab);

        ShowLobbyTab();
    }

    private void CreateToolbar()
    {
        Toolbar toolbar = new Toolbar();
        toolbar.style.flexShrink = 0;
        lobbyTabButton = new ToolbarToggle { text = "Lobby" };
        lobbyTabButton.style.flexGrow = 1;

        lobbyTabButton.RegisterValueChangedCallback(eventData =>
            {
                if (eventData.newValue)
                {
                    ShowLobbyTab();
                }
            }
        );

        networkSimulatorTabButton = new ToolbarToggle { text = "Network Simulator" };
        networkSimulatorTabButton.style.flexGrow = 1;

        networkSimulatorTabButton.RegisterValueChangedCallback(eventData =>
            {
                if (eventData.newValue)
                {
                    ShowNetworkSimulatorTab();
                }
            }
        );

        toolbar.Add(lobbyTabButton);
        toolbar.Add(networkSimulatorTabButton);
        rootVisualElement.Add(toolbar);
    }

    private void CreateLobbyTab()
    {
        lobbyTab = new VisualElement();
        lobbyTab.style.flexGrow = 1;

        IMGUIContainer lobbyContainer = new IMGUIContainer(() => { multiplayerDebugDrawer.Draw(lobbyManager, null, sceneControllerManager); });

        lobbyContainer.style.flexGrow = 1;
        lobbyTab.Add(lobbyContainer);
    }

    private void ShowLobbyTab()
    {
        lobbyTabButton.SetValueWithoutNotify(true);
        networkSimulatorTabButton.SetValueWithoutNotify(false);
        lobbyTab.style.display = DisplayStyle.Flex;
        networkSimulatorTab.style.display = DisplayStyle.None;
    }

    private void CreateNetworkSimulatorTab()
    {
        networkSimulatorTab = new ScrollView();
        networkSimulatorTab.style.flexGrow = 1;
        networkSimulatorTab.style.display = DisplayStyle.None;

        Editor networkSimulatorEditor = Editor.CreateEditor(networkSimulator);
        InspectorElement inspector = new InspectorElement(networkSimulatorEditor);
        inspector.style.flexGrow = 1;

        networkSimulatorTab.Add(inspector);
    }

    private void ShowNetworkSimulatorTab()
    {
        lobbyTabButton.SetValueWithoutNotify(false);
        networkSimulatorTabButton.SetValueWithoutNotify(true);
        lobbyTab.style.display = DisplayStyle.None;
        networkSimulatorTab.style.display = DisplayStyle.Flex;
    }
}
