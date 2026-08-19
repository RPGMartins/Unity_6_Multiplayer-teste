using UnityEngine;

public class PlayerInitializerSingleplayer : MonoBehaviour
{
    [SerializeField] private PlayerInitializer playerInitializer;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private InventoryManager inventoryManager;

    public void SpawnPlayer(ulong clientID)
    {
        IPlayerController playerController = playerInitializer.SpawnPlayer(playerSpawner, string.Empty, clientID);
        playerInitializer.InitializePlayerInventory(inventoryManager, playerController, playerController.AuthenticateID);
        inventoryManager.FinishInitialization();
    }
}
