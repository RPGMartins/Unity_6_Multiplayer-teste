using UnityEngine;

public class LobbyUiMock : MonoBehaviour
{
    [SerializeField] private LobbyManager lobbyManager;

    public LobbyManager LobbyManager => lobbyManager;
}
