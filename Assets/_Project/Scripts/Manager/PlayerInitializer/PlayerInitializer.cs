using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    private static string DEFAULT_PLAYER_ID = "DefaultPlayer";
    private static string DEFAULT_FlashLight_ID = "FlashLight";
    private static string DEFAULT_Pistol_ID = "Pistol";

    private List<IPlayerController> createdPlayers = new List<IPlayerController>();

    public IPlayerController SpawnPlayer(IPlayerSpawner playerSpawner, FixedString64Bytes playerID, ulong clientID)
    {
        if (playerID == string.Empty)
        {
            playerID = DEFAULT_PLAYER_ID;
        }

        IPlayerController playerController = playerSpawner.SpawnPlayer(clientID);
        createdPlayers.Add(playerController);
        InitializePlayer(playerController, playerID, clientID);

        return playerController;
    }

    public void InitializePlayerInventory(IInventoryManager inventoryManager, IPlayerController playerController, FixedString64Bytes playerID)
    {
        inventoryManager.ServerAddPlayer(playerID);
        inventoryManager.OnInventoryChanged += (tempPlayerID) => InventoryChanged(playerController, tempPlayerID);

        WeaponItem inventoryItemFlashlight = ItemDataList.Instance.GetItemById(DEFAULT_FlashLight_ID) as WeaponItem;
        WeaponItem inventoryItemPistol = ItemDataList.Instance.GetItemById(DEFAULT_Pistol_ID) as WeaponItem;

        inventoryManager.ServerAddItem(inventoryItemFlashlight, playerID);
        inventoryManager.ServerAddItem(inventoryItemPistol, playerID);
    }

    private void InitializePlayer(IPlayerController playerController, FixedString64Bytes playerID, ulong clientID)
    {
        playerController.Initialize(playerID, clientID);
    }

    private void InventoryChanged(IPlayerController playerController, FixedString64Bytes playerID)
    {
        if (playerID == playerController.AuthenticateID)
        {
            playerController.PlayerLocalInventoryController.onLocalInventoryChanged?.Invoke();
        }
    }

    public void SearchAndInitializePlayer(FixedString64Bytes playerID, ulong clientID)
    {
        foreach (IPlayerController playerController in createdPlayers)
        {
            if (playerController.AuthenticateID == playerID)
            {
                InitializePlayer(playerController, playerID, clientID);

                break;
            }
        }
    }
}
