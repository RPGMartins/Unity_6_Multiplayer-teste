using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    public PlayerController SpawnPlayer(IInventoryManager inventoryManager, ulong clientID, bool network)
    {
        PlayerController playerController = SpawnPlayer(clientID, network);
        inventoryManager.OnInventoryChanged += (tempPlayerID) => InventoryChanged(playerController, tempPlayerID);

        return playerController;
    }

    private void InventoryChanged(PlayerController playerController, FixedString64Bytes playerID)
    {
        if (playerID == playerController.AuthenticateID)
        {
            playerController.PlayerLocalInventoryController.onLocalInventoryChanged?.Invoke();
        }
    }

    private PlayerController SpawnPlayer(ulong clientID, bool network)
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
