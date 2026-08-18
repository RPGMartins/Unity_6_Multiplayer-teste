using UnityEngine;

public class PlayerSpawnerController : MonoBehaviour
{
    public PlayerController SpawnPlayer(GameObject playerPrefab)
    {
        GameObject playerInstance = Instantiate(playerPrefab);
        PlayerController playerController = playerInstance.GetComponentInChildren<PlayerController>();

        return playerController;
    }
}
