using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private List<PlayerController> createdPlayers = new List<PlayerController>();

    public PlayerController SpawnPlayer(IInventoryManager inventoryManager, ulong clientID, bool network)
    {
        PlayerController playerController = SpawnPlayer(clientID, network);
        createdPlayers.Add(playerController);
        inventoryManager.OnInventoryChanged += (tempPlayerID) => InventoryChanged(playerController, tempPlayerID);

        return playerController;
    }

    public void InitializeDefaultInventoryToPlayer(IInventoryManager inventoryManager, FixedString64Bytes playerID)
    {
        WeaponItem inventoryItemFlashlight = ItemDataList.Instance.GetItemById(PlayerInitializer.DEFAULT_FlashLight_ID) as WeaponItem;
        WeaponItem inventoryItemPistol = ItemDataList.Instance.GetItemById(PlayerInitializer.DEFAULT_Pistol_ID) as WeaponItem;

        inventoryManager.ServerAddItem(inventoryItemFlashlight, playerID);
        inventoryManager.ServerAddItem(inventoryItemPistol, playerID);
    }

    private void InventoryChanged(PlayerController playerController, FixedString64Bytes playerID)
    {
        if (playerID == playerController.AuthenticateID)
        {
            playerController.PlayerLocalInventoryController.onLocalInventoryChanged?.Invoke();
        }
    }

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
