using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;

public class SessionUiMock : MonoBehaviour
{
    [SerializeField] protected SessionManager sessionManager;
    [SerializeField] private AuthenticationManager authenticationManager;
    [SerializeField] private LobbyUIManager lobbyUIManager;

    public LobbyUIManager LobbyUIManager => lobbyUIManager;
    public SessionManager SessionManager => sessionManager;

    private void Awake()
    {
        authenticationManager.OnAuthenticatedSucceed += DebugAuthenticatedSucceed;
        authenticationManager.OnAuthenticatedFailed += DebugAuthenticatedFailed;
        sessionManager.OnSessionLostConnection += DebugLostConnection;
    }

    private void DebugLostConnection(ISession session)
    {
        Debug.Log($"OnSessionLostConnection");
    }

    private void DebugAuthenticatedSucceed(IAuthenticationService authenticationService)
    {
        Debug.Log("OnAuthenticatedSucceed");
    }

    private void DebugAuthenticatedFailed()
    {
        Debug.Log("OnAuthenticatedFailed");
    }
}
