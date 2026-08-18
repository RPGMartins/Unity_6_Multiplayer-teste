using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class PlayerInitializerUtils : MonoBehaviour
{
    private static string DEFAULT_PLAYER_ID = "DefaultPlayer";
    private static string DEFAULT_FlashLight_ID = "FlashLight";
    private static string DEFAULT_Pistol_ID = "Pistol";

    [SerializeField] private List<PlayerController> createdPlayers = new List<PlayerController>();

    public void SpawnPlayer(IPlayerSpawner playerSpawner, IInventoryManager inventoryManager, FixedString64Bytes playerID, ulong clientID)
    {
        if (playerID == string.Empty)
        {
            playerID = DEFAULT_PLAYER_ID;
        }

        PlayerController playerController = playerSpawner.SpawnPlayer(clientID);
        createdPlayers.Add(playerController);
        InitializePlayer(playerController, playerID, clientID);
        InitializePlayerInventory(inventoryManager, playerController, playerID);
    }

    private void InitializePlayerInventory(IInventoryManager inventoryManager, PlayerController playerController, FixedString64Bytes playerID)
    {
        inventoryManager.AddPlayer(playerID);
        inventoryManager.OnInventoryChanged += (tempPlayerID) => InventoryChanged(playerController, tempPlayerID);

        WeaponItem inventoryItemFlashlight = ItemDataList.Instance.GetItemById(DEFAULT_FlashLight_ID) as WeaponItem;
        WeaponItem inventoryItemPistol = ItemDataList.Instance.GetItemById(DEFAULT_Pistol_ID) as WeaponItem;

        inventoryManager.AddItemToPlayer(inventoryItemFlashlight, playerID);
        inventoryManager.AddItemToPlayer(inventoryItemPistol, playerID);
    }

    private void InitializePlayer(PlayerController playerController, FixedString64Bytes playerID, ulong clientID)
    {
        playerController.Initialize(playerID, clientID);
    }

    private void InventoryChanged(PlayerController playerController, FixedString64Bytes playerID)
    {
        if (playerID == playerController.AuthenticateID)
        {
            playerController.PlayerLocalInventoryController.onLocalInventoryChanged?.Invoke();
        }
    }

    public void SearchAndInitializePlayer(FixedString64Bytes playerID, ulong clientID)
    {
        foreach (PlayerController playerController in createdPlayers)
        {
            if (playerController.AuthenticateID == playerID)
            {
                InitializePlayer(playerController, playerID, clientID);

                break;
            }
        }
    }
}
