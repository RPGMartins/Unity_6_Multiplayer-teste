using System;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ManageNetworkUI : MonoBehaviour
{
    private static readonly string[] serverNames = new[] { "Ands Server", "Kkesls Server", " Matrins Server" };

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

        sessionManager.OnSessionInitialized += OpenSessionViewerUI;
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
        lobbyViwerUI.Initialize(session);
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
        SessionOptions options = CreateSessionOptions().WithRelayNetwork();
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

    public static SessionOptions CreateSessionOptions(string name = "", bool isPrivate = false, int maxPlayers = -1, string password = "")
    {
        if (name == string.Empty)
        {
            name = GetRandomLobbyName();
        }

        if (maxPlayers == -1)
        {
            maxPlayers = Random.Range(3, 8);
        }

        SessionOptions options = new SessionOptions
        {
            Type = "Lobby",
            Name = name,
            MaxPlayers = maxPlayers,
            IsPrivate = isPrivate,
            Password = string.IsNullOrWhiteSpace(password) ? null : password
        };

        return options;
    }

    private static string GetRandomLobbyName()
    {
        return serverNames[Random.Range(0, serverNames.Length)];
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
