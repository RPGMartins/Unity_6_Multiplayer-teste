using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    public PlayerController SpawnPlayer(ulong clientID, bool network)
    {
        PlayerController playerController = null;

        if (network == true)
        {
            playerController = SpawnPrefab();
            NetworkObject instanceNetworkObject = playerController.Transform.GetComponentInParent<NetworkObject>();
            instanceNetworkObject.SpawnAsPlayerObject(clientID, true);
        }
        else
        {
            playerController = SpawnPrefab();
        }

        return playerController;
    }

    private PlayerController SpawnPrefab()
    {
        GameObject playerInstance = Instantiate(playerPrefab);
        PlayerController playerController = playerInstance.GetComponentInChildren<PlayerController>();

        return playerController;
    }
}
