using System;
using UnityEngine;

public class SessionUiMock : MonoBehaviour
{
    [SerializeField] protected SessionManager sessionManager;

    [SerializeField] private LobbyManager lobbyManager;

    public LobbyManager LobbyManager => lobbyManager;
    public SessionManager SessionManager => sessionManager;

    private void Awake()
    {
        sessionManager.OnAuthenticatedSucceed += SessionManagerOnOnAuthenticatedSucceed;
        sessionManager.OnAuthenticatedFailed += SessionManagerOnOnAuthenticatedFailed;
        sessionManager.OnSessionLostConnection += SessionManagerOnOnSessionLostConnection;
    }

    private void SessionManagerOnOnSessionLostConnection()
    {
        Debug.Log($"OnSessionLostConnection");
    }

    private void SessionManagerOnOnAuthenticatedSucceed()
    {
        Debug.Log("OnAuthenticatedSucceed");
    }

    private void SessionManagerOnOnAuthenticatedFailed()
    {
        Debug.Log("OnAuthenticatedFailed");
    }
}
