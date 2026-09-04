using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    public static string DEFAULT_PLAYER_ID = "DefaultPlayer";
    public static string DEFAULT_FlashLight_ID = "FlashLight";
    public static string DEFAULT_Pistol_ID = "Pistol";

    public static PlayerInitializer instance;

    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private NetworkInventoryManager networkInventoryManager;

    private List<FixedString64Bytes> clientsToCreate = new List<FixedString64Bytes>();
    private List<FixedString64Bytes> clientsCreated = new List<FixedString64Bytes>();
    private List<FixedString64Bytes> clientsInventoryToInitialize = new List<FixedString64Bytes>();
    private List<FixedString64Bytes> clientsInventoryInitialized = new List<FixedString64Bytes>();

    private LobbyManager lobbyManager;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        lobbyManager = LobbyManager.Instance;

        if (NetworkConnectionChecker.CheckConnection() == true)
        {
            return;
        }

        if (clientsToCreate.Count <= 0)
        {
            PreparePlayersToBeInitialized(new FixedString64Bytes[] { DEFAULT_PLAYER_ID });
        }
    }

    private void Update()
    {
        if (NetworkConnectionChecker.CheckIfHasAuthority() == true)
        {
            InitializeInventoryOnline();
            SpawnOnline();
        }
        else
        {
            InitializeInventoryOffline();
            SpawnOffline();
        }
    }

    private void SpawnOffline()
    {
        for (int i = clientsToCreate.Count - 1; i >= 0; i--)
        {
            ulong clientID = 0;
            FixedString64Bytes playerID = clientsToCreate[i];
            PlayerController playerController = SpawnPlayer(clientID, i, false);
            playerController.Initialize(playerID, clientID);
        }
    }

    private void InitializeInventoryOffline()
    {
        for (int i = clientsInventoryToInitialize.Count - 1; i >= 0; i--)
        {
            playerSpawner.InitializeDefaultInventoryToPlayer(inventoryManager, clientsInventoryToInitialize[i]);
            clientsInventoryInitialized.Add(clientsInventoryToInitialize[i]);
            clientsInventoryToInitialize.RemoveAt(i);
        }
    }

    private void SpawnOnline()
    {
        for (int i = clientsToCreate.Count - 1; i >= 0; i--)
        {
            ulong clientID = 0;
            FixedString64Bytes playerID = clientsToCreate[i];
            List<PlayerData> lobbyPlayers = lobbyManager.GetLobbyPlayers();

            foreach (PlayerData playerData in lobbyPlayers)
            {
                if (playerData.AuthenticationID != clientsToCreate[i])
                {
                    continue;
                }

                clientID = playerData.ClientId;

                break;
            }

            NetworkInventoryManager.Instance.ServerAddPlayer(clientsToCreate[i]);
            PlayerController playerController = SpawnPlayer(clientID, i, true);
            playerController.InitializeRPC(playerID, clientID);
        }
    }

    private void InitializeInventoryOnline()
    {
        for (int i = clientsInventoryToInitialize.Count - 1; i >= 0; i--)
        {
            playerSpawner.InitializeDefaultInventoryToPlayer(networkInventoryManager, clientsInventoryToInitialize[i]);
            clientsInventoryInitialized.Add(clientsInventoryToInitialize[i]);
            clientsInventoryToInitialize.RemoveAt(i);
            RebuildLocalInventoryRPC();
        }
    }

    private PlayerController SpawnPlayer(ulong clientID, int index, bool network)
    {
        PlayerController playerController = playerSpawner.SpawnPlayer(inventoryManager, clientID, network);
        clientsCreated.Add(clientsToCreate[index]);
        clientsToCreate.RemoveAt(index);

        return playerController;
    }

    #region RPC

    [Rpc(SendTo.NotMe)]
    private void RebuildLocalInventoryRPC()
    {
        networkInventoryManager.RebuildLocalInventory();
    }

    [Rpc(SendTo.Server)]
    public void InitializeInventoryRPC(FixedString64Bytes clientsIDs)
    {
        clientsInventoryToInitialize.Add(clientsIDs);
    }

    public void PreparePlayersToBeInitialized(FixedString64Bytes[] clientsIDs)
    {
        clientsToCreate.AddRange(clientsIDs);
    }

    #endregion
}
