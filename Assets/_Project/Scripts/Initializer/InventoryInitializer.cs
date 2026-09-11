using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class InventoryInitializer : NetworkBehaviour
{
    public static InventoryInitializer instance;

    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private List<WeaponItemData> weaponsToAddToPlayerWhenEnter;

    private List<FixedString64Bytes> clientsInventoryToInitialize = new List<FixedString64Bytes>();
    private List<FixedString64Bytes> clientsInventoryInitialized = new List<FixedString64Bytes>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        PlayerInitializer.instance.OnPlayerInitialized += InitializeInventory;
    }

    private void OnDestroy()
    {
        if (PlayerInitializer.instance != null)
        {
            PlayerInitializer.instance.OnPlayerInitialized -= InitializeInventory;
        }
    }

    private void Update()
    {
        if (NetworkConnectionChecker.CheckIfHasAuthority() == true)
        {
            InitializeInventoryNetwork();
        }
        else
        {
            InitializeInventoryLocal();
        }
    }

    private void InitializeInventoryLocal()
    {
        for (int i = clientsInventoryToInitialize.Count - 1; i >= 0; i--)
        {
            InitializeInventory(i);
        }
    }

    private void InitializeInventoryNetwork()
    {
        for (int i = clientsInventoryToInitialize.Count - 1; i >= 0; i--)
        {
            InitializeInventory(i);
            RebuildLocalInventoryRPC();
        }
    }

    private void InitializeInventory(int index)
    {
        FixedString64Bytes clientsInventoryInitialize = clientsInventoryToInitialize[index];
        InitializeDefaultInventoryToPlayer(inventoryManager, clientsInventoryInitialize);
        clientsInventoryInitialized.Add(clientsInventoryToInitialize[index]);
        clientsInventoryToInitialize.RemoveAt(index);
    }

    private void InitializeDefaultInventoryToPlayer(InventoryManager inventoryManager, FixedString64Bytes playerID)
    {
        foreach (WeaponItemData weaponItemData in weaponsToAddToPlayerWhenEnter)
        {
            inventoryManager.TryAddItem(weaponItemData.ToBaseItem(), playerID);
        }

        inventoryManager.TryEquipItem(new InventoryData(playerID, weaponsToAddToPlayerWhenEnter[1].ItemID, true));
    }

    private void InitializeInventory(FixedString64Bytes clientsIDs)
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
        inventoryManager.RebuildLocalInventory();
    }

    #endregion
}
