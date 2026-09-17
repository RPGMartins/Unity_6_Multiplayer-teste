using System;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ManageNetworkUI : MonoBehaviour
{
    [SerializeField] private LobbiesUI lobbiesUI;
    [SerializeField] private LobbyViwerUI lobbyViwerUI;
    [SerializeField] private ErrorScreenUI errorScreenUI;
    [SerializeField] private RectTransform networkUIRect;
    [SerializeField] private RectTransform loadScreen;
    [SerializeField] private JoinLobbyScreen joinLobbyScreen;
    [SerializeField] private CreateLobbyScreen createLobbyScreen;
    [SerializeField] private MenuLayout menuLayout;
    private SessionManager sessionManager;
    private bool isVisible;

    private void Start()
    {
        sessionManager = SessionManager.Instance;

        sessionManager.OnStartTryConnectToSession += OpenLoadScreen;
        sessionManager.OnStartTryDisconnectFromSession += OpenLoadScreen;

        sessionManager.OnSessionConnected += OpenSessionViewerUI;
        sessionManager.OnSessionDisconnect += OpenMainMenuNetworkSession;

        sessionManager.OnSessionConnectFailed += OpenErrorScreenUI;
        sessionManager.OnSessionDisconnectFailed += OpenErrorScreenUI;

        OpenMainMenuNetwork();
    }

    private void Update()
    {
        menuLayout.CheckIfInLobby();

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            isVisible = !isVisible;

            if (isVisible == true)
            {
                networkUIRect.gameObject.SetActive(true);

                if (NetworkManager.Singleton.IsConnectedClient == true)
                {
                    lobbyViwerUI.OpenScreen();
                }

                OpenMainMenuNetwork();
            }
            else
            {
                networkUIRect.gameObject.SetActive(false);
                CloseAll();
            }
        }
    }

    public void OpenLobbiesUI()
    {
        CloseAll();
        lobbiesUI.OpenScreen();
        lobbiesUI.RefreshLobbies();
    }

    public void OpenMainMenuNetwork()
    {
        CloseAll();
    }

    public void OpenJoinLobbyScreen()
    {
        CloseAll();
        joinLobbyScreen.OpenScreen();
    }

    public void OpenCreateLobbyScreen()
    {
        CloseAll();
        createLobbyScreen.OpenScreen();
    }

    private void OpenMainMenuNetworkSession(GameSession session)
    {
        OpenMainMenuNetwork();
    }

    private void OpenLoadScreen()
    {
        CloseAll();
        loadScreen.gameObject.SetActive(true);
    }

    public void OpenSessionViewerUI(GameSession session)
    {
        OpenLobbyViewerUINetwork();
        lobbyViwerUI.Initialize();
    }

    public void OpenLobbyViewerUINetwork()
    {
        CloseAll();
        lobbyViwerUI.OpenScreen();
    }

    private void OpenErrorScreenUI(SessionError error)
    {
        CloseAll();
        errorScreenUI.OpenScreen();
        errorScreenUI.Initialize(error);
    }

    public void QuickJoinLobby()
    {
        sessionManager.QuickJoinSession();
    }

    public void CreateRandomLobby()
    {
        SessionOptions options = MultiplayerDebugDrawer.CreateSessionOptions().WithRelayNetwork();
        sessionManager.CreateSession(options);
    }

    private void CloseAll()
    {
        loadScreen.gameObject.SetActive(false);
        lobbiesUI.CloseScreen();
        lobbyViwerUI.CloseScreen();
        errorScreenUI.CloseScreen();
        joinLobbyScreen.CloseScreen();
        createLobbyScreen.CloseScreen();
    }
}

[Serializable]
public struct MenuLayout
{
    [SerializeField] private Button viewLobby;
    [SerializeField] private Button createRandomLobby;
    [SerializeField] private Button joinLobby;
    [SerializeField] private Button quickJoinLobby;
    [SerializeField] private Button searchLobby;

    public void CheckIfInLobby()
    {
        bool show = NetworkManager.Singleton?.IsConnectedClient == true;

        createRandomLobby.gameObject.SetActive(!show);
        joinLobby.gameObject.SetActive(!show);
        quickJoinLobby.gameObject.SetActive(!show);
        searchLobby.gameObject.SetActive(!show);

        viewLobby.gameObject.SetActive(show);
    }
}
