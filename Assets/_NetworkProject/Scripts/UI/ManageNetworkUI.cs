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
    private LobbyManager lobbyManager;
    private bool isVisible;

    private void Start()
    {
        lobbyManager = LobbyManager.Instance;

        lobbyManager.OnStartTryConnectToSession += OpenLoadScreen;
        lobbyManager.OnStartTryDisconnectFromSession += OpenLoadScreen;

        lobbyManager.OnSessionConnected += OpenLobbyViewerUI;
        lobbyManager.OnSessionDisconnect += OpenMainMenuNetworkSession;

        lobbyManager.OnSessionConnectFailed += OpenErrorScreenUI;
        lobbyManager.OnSessionDisconnectFailed += OpenErrorScreenUI;

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

    private void OpenMainMenuNetworkSession(ISession session)
    {
        OpenMainMenuNetwork();
    }

    private void OpenLoadScreen()
    {
        CloseAll();
        loadScreen.gameObject.SetActive(true);
    }

    public void OpenLobbyViewerUI(ISession session)
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
        lobbyManager.QuickJoinSession();
    }

    public void CreateRandomLobby()
    {
        SessionOptions options = MultiplayerDebugDrawer.CreateSessionOptions().WithRelayNetwork();
        lobbyManager.CreateSession(options);
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
    [SerializeField] private Button updateLobby;
    [SerializeField] private Button viewLobby;
    [SerializeField] private Button createRandomLobby;
    [SerializeField] private Button joinLobby;
    [SerializeField] private Button quickJoinLobby;
    [SerializeField] private Button serachLobby;

    public void CheckIfInLobby()
    {
        if (NetworkManager.Singleton.IsConnectedClient == false)
        {
            createRandomLobby.gameObject.SetActive(true);
            joinLobby.gameObject.SetActive(true);
            quickJoinLobby.gameObject.SetActive(true);
            serachLobby.gameObject.SetActive(true);
            updateLobby.gameObject.SetActive(true);
            viewLobby.gameObject.SetActive(false);

            return;
        }

        bool show = NetworkManager.Singleton.IsConnectedClient;

        createRandomLobby.gameObject.SetActive(!show);
        joinLobby.gameObject.SetActive(!show);
        quickJoinLobby.gameObject.SetActive(!show);
        serachLobby.gameObject.SetActive(!show);

        updateLobby.gameObject.SetActive(show);
        viewLobby.gameObject.SetActive(show);
    }
}
