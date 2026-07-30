using System;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine;

public class SessionUI : MonoBehaviour
{
    [SerializeField] private HostUI hostUI;
    [SerializeField] private ClientUI clientUI;
    [SerializeField] private LobbyUI lobbyUI;

    private SessionManager sessionManager;

    private void Awake()
    {
        sessionManager = FindAnyObjectByType<SessionManager>();

        sessionManager.OnSessionConnected += CreateSession;
        sessionManager.OnSessionEnded += LeaveSession;
    }

    private void Start()
    {
        lobbyUI.gameObject.SetActive(false);
    }

    public void Initialize(Func<SessionOptions, Task> hostCallback, Func<string, Task> clientCallback)
    {
        hostUI.Initialize(hostCallback);
        clientUI.Initialize(clientCallback);
    }

    private void CreateSession(ISession session)
    {
        hostUI.gameObject.SetActive(false);
        clientUI.gameObject.SetActive(false);
        lobbyUI.gameObject.SetActive(true);
    }

    private void LeaveSession()
    {
        hostUI.gameObject.SetActive(true);
        clientUI.gameObject.SetActive(true);
        lobbyUI.gameObject.SetActive(false);

        lobbyUI.ResetVariables();
    }
}
