using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    public static string DEFAULT_PLAYER_ID = "DefaultPlayer";
    private static string DEFAULT_FlashLight_ID = "FlashLight";
    private static string DEFAULT_Pistol_ID = "Pistol";

    private List<IPlayerController> createdPlayers = new List<IPlayerController>();

    public IPlayerController SpawnPlayer(IInventoryManager inventoryManager, IPlayerSpawner playerSpawner, ulong clientID)
    {
        IPlayerController playerController = playerSpawner.SpawnPlayer(clientID);
        createdPlayers.Add(playerController);
        InitializePlayerInventory(inventoryManager, playerController);

        return playerController;
    }

    private void InitializePlayerInventory(IInventoryManager inventoryManager, IPlayerController playerController)
    {
        inventoryManager.OnInventoryChanged += (tempPlayerID) => InventoryChanged(playerController, tempPlayerID);
    }

    public void InitializeDefaultInventoryToPlayer(IInventoryManager inventoryManager, FixedString64Bytes playerID)
    {
        WeaponItem inventoryItemFlashlight = ItemDataList.Instance.GetItemById(DEFAULT_FlashLight_ID) as WeaponItem;
        WeaponItem inventoryItemPistol = ItemDataList.Instance.GetItemById(DEFAULT_Pistol_ID) as WeaponItem;

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
}
