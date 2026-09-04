using System;
using UnityEngine;

public class PlayerInitializerSinglePlayer : MonoBehaviour
{
    [SerializeField] private PlayerInitializer playerInitializer;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private InventoryManager inventoryManager;

    public void SpawnPlayer(ulong clientID)
    {
        IPlayerController playerController = playerInitializer.SpawnPlayer(inventoryManager, playerSpawner, clientID);
        playerInitializer.InitializeDefaultInventoryToPlayer(inventoryManager, PlayerInitializer.DEFAULT_PLAYER_ID);
    }
}
