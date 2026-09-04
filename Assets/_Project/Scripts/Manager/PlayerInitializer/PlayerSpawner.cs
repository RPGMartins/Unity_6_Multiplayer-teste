using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerPrefabNetwork;

    private List<IPlayerController> createdPlayers = new List<IPlayerController>();

    public IPlayerController SpawnPlayer(IInventoryManager inventoryManager, ulong clientID, bool network)
    {
        IPlayerController playerController = SpawnPlayer(clientID, network);
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

    private void InventoryChanged(IPlayerController playerController, FixedString64Bytes playerID)
    {
        if (playerID == playerController.AuthenticateID)
        {
            playerController.PlayerLocalInventoryController.onLocalInventoryChanged?.Invoke();
        }
    }

    public IPlayerController SpawnPlayer(ulong clientID, bool network)
    {
        IPlayerController playerController = null;

        if (network == true)
        {
            playerController = SpawnPrefab(playerPrefabNetwork);
            NetworkObject instanceNetworkObject = playerController.Transform.GetComponentInParent<NetworkObject>();
            instanceNetworkObject.SpawnAsPlayerObject(clientID, true);
        }
        else
        {
            playerController = SpawnPrefab(playerPrefab);
        }

        return playerController;
    }

    private IPlayerController SpawnPrefab(GameObject tempPrefab)
    {
        GameObject playerInstance = Instantiate(tempPrefab);
        IPlayerController playerController = playerInstance.GetComponentInChildren<IPlayerController>();

        return playerController;
    }
}
