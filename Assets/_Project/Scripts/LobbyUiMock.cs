using UnityEngine;

public class LobbyUiMock : MonoBehaviour
{
    [SerializeField] private SessionManager sessionManager;

    [SerializeField] private LobbyManager lobbyManager;

    public LobbyManager LobbyManager => lobbyManager;
    public SessionManager SessionManager => sessionManager;
}
