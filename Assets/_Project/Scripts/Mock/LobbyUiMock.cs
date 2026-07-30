using UnityEngine;

public class LobbyUiMock : MonoBehaviour
{
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private LobbyManager lobbyManager;

    public SessionManager SessionManager => sessionManager;
    public LobbyManager LobbyManager => lobbyManager;
}
