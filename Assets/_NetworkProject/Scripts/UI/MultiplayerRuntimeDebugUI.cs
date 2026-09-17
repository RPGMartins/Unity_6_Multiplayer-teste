using Unity.Multiplayer.Tools.NetworkSimulator.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiplayerRuntimeDebugUI : MonoBehaviour
{
    [Header("Interface")]
    [SerializeField][Range(0.5f, 3f)]
    private float uiScale = 1.5f;

    [SerializeField]
    private Vector2 panelPosition = new Vector2(10f, 10f);

    [SerializeField]
    private Vector2 panelSize = new Vector2(500f, 700f);

    private SessionManager sessionManager;
    private NetworkSimulator networkSimulator;
    private SceneControllerManager sceneControllerManager;
    private MultiplayerDebugDrawer sharedDrawer;

    private bool isVisible = true;
    private Vector2 scrollPosition;

    private void Awake()
    {
        sessionManager = FindAnyObjectByType<SessionManager>();
        networkSimulator = FindAnyObjectByType<NetworkSimulator>();
        sceneControllerManager = FindAnyObjectByType<SceneControllerManager>();

        sharedDrawer = new MultiplayerDebugDrawer();
    }

    private void Update()
    {
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            isVisible = !isVisible;
        }
    }

    private void OnGUI()
    {
        if (isVisible == false)
        {
            return;
        }

        Matrix4x4 previousMatrix = GUI.matrix;

        float validScale = Mathf.Max(0.1f, uiScale);

        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(validScale, validScale, 1f));

        float maximumWidth = Screen.width / validScale - panelPosition.x - 10f;
        float maximumHeight = Screen.height / validScale - panelPosition.y - 10f;
        float panelWidth = Mathf.Clamp(panelSize.x, 250f, maximumWidth);
        float panelHeight = Mathf.Clamp(panelSize.y, 250f, maximumHeight);

        GUILayout.BeginArea(new Rect(panelPosition.x, panelPosition.y, panelWidth, panelHeight), "Multiplayer Debug", GUI.skin.window);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        sharedDrawer.Draw(sessionManager, networkSimulator, sceneControllerManager);

        GUILayout.Space(10f);
        GUILayout.Label("F1: mostrar/esconder");

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        GUI.matrix = previousMatrix;
    }
}
