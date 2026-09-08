using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class InventoryInitializer : NetworkBehaviour
{
    public static InventoryInitializer instance;

    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private NetworkInventoryManager networkInventoryManager;
    [SerializeField] private List<WeaponItemData> weaponsToAddToPlayerWhenEnter;

    private List<FixedString64Bytes> clientsInventoryToInitialize = new List<FixedString64Bytes>();
    private List<FixedString64Bytes> clientsInventoryInitialized = new List<FixedString64Bytes>();

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (NetworkConnectionChecker.CheckIfHasAuthority() == true)
        {
            InitializeInventoryOnline();
        }
        else
        {
            InitializeInventoryOffline();
        }
    }

    private void InitializeInventoryOffline()
    {
        for (int i = clientsInventoryToInitialize.Count - 1; i >= 0; i--)
        {
            InitializeInventory(i, inventoryManager);
        }
    }

    private void InitializeInventoryOnline()
    {
        for (int i = clientsInventoryToInitialize.Count - 1; i >= 0; i--)
        {
            InitializeInventory(i, networkInventoryManager);
            RebuildLocalInventoryRPC();
        }
    }

    private void InitializeInventory(int index, IInventoryManager interfaceInventoryManager)
    {
        FixedString64Bytes clientsInventoryInitialize = clientsInventoryToInitialize[index];
        InitializeDefaultInventoryToPlayer(interfaceInventoryManager, clientsInventoryInitialize);
        clientsInventoryInitialized.Add(clientsInventoryToInitialize[index]);
        clientsInventoryToInitialize.RemoveAt(index);
        inventoryManager.OnInventoryChanged += InventoryChanged;
    }

    private void InventoryChanged(FixedString64Bytes playerID)
    {
        PlayerController playerController = PlayerInitializer.instance.GetPlayerControllerByPlayerID(playerID);

        if (playerController != null)
        {
            playerController.PlayerLocalInventoryController.onLocalInventoryChanged?.Invoke();
        }
    }

    private void InitializeDefaultInventoryToPlayer(IInventoryManager inventoryManager, FixedString64Bytes playerID)
    {
        foreach (WeaponItemData weaponItemData in weaponsToAddToPlayerWhenEnter)
        {
            inventoryManager.ServerAddItem(weaponItemData.ToBaseItem(), playerID);
        }
    }

    public void InitializeInventory(FixedString64Bytes clientsIDs)
    {
        if (NetworkConnectionChecker.CheckIfHasAuthority() == true)
        {
            InitializeInventoryRPC(clientsIDs);
        }
        else
        {
            clientsInventoryToInitialize.Add(clientsIDs);
        }
    }

    #region RPC

    [Rpc(SendTo.Server)]
    private void InitializeInventoryRPC(FixedString64Bytes clientsIDs)
    {
        clientsInventoryToInitialize.Add(clientsIDs);
    }

    [Rpc(SendTo.NotMe)]
    private void RebuildLocalInventoryRPC()
    {
        networkInventoryManager.RebuildLocalInventory();
    }

    #endregion
}
