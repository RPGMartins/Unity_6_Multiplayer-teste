using UnityEngine;

public class SessionUiMock : MonoBehaviour
{
    [SerializeField] private LobbyManager lobbyManager;

    public LobbyManager LobbyManager => lobbyManager;
}
