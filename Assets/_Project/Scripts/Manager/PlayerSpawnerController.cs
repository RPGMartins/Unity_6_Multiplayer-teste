using UnityEngine;

public class PlayerSpawnerController : MonoBehaviour
{
    public IPlayerController SpawnPlayer(GameObject playerPrefab)
    {
        GameObject playerInstance = Instantiate(playerPrefab);
        IPlayerController playerController = playerInstance.GetComponentInChildren<IPlayerController>();

        return playerController;
    }
}
