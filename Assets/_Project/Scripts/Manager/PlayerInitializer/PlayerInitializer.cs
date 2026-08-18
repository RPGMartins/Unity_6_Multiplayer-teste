using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    [SerializeField] private PlayerInitializerUtils playerInitializerUtils;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private InventoryManager inventoryManager;

    public void SpawnPlayer(ulong clientID)
    {
        playerInitializerUtils.SpawnPlayer(playerSpawner, inventoryManager, string.Empty, clientID);
    }
}
