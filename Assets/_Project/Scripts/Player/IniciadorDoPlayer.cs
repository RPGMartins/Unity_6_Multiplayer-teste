using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class IniciadorDoPlayer : MonoBehaviour, IIniciadorDoPlayer
{
    public static string DEFAULT_PLAYER_ID = "DefaultPlayer";
    public static IniciadorDoPlayer instance;

    public event Action OnDefaultInventoryAdded;

    [SerializeField] private PlayerInitializer playerInitializer;
    [SerializeField] private PlayerInitializerNetwork playerInitializerNetwork;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private PlayerSpawnerNetwork playerSpawnerNetwork;
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
            InitializePlayersRPC(new FixedString64Bytes[] { DEFAULT_PLAYER_ID });
        }
    }

    private void Update() // tirar do update
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
            playerInitializer.SpawnPlayer(inventoryManager, playerSpawner, clientID);
            clientsCreated.Add(clientsToCreate[i]);
            clientsToCreate.RemoveAt(i);
        }
    }

    private void InitializeInventoryOffline()
    {
        for (int i = clientsInventoryToInitialize.Count - 1; i >= 0; i--)
        {
            playerInitializer.InitializeDefaultInventoryToPlayer(inventoryManager, clientsInventoryToInitialize[i]);
            clientsInventoryInitialized.Add(clientsInventoryToInitialize[i]);
            clientsInventoryToInitialize.RemoveAt(i);
        }
    }

    private void SpawnOnline()
    {
        for (int i = clientsToCreate.Count - 1; i >= 0; i--)
        {
            ulong clientID = 0;

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

            SpawnPlayer(clientsToCreate[i], clientID);
            clientsCreated.Add(clientsToCreate[i]);
            clientsToCreate.RemoveAt(i);
        }
    }

    private void InitializeInventoryOnline()
    {
        for (int i = clientsInventoryToInitialize.Count - 1; i >= 0; i--)
        {
            playerInitializer.InitializeDefaultInventoryToPlayer(networkInventoryManager, clientsInventoryToInitialize[i]);
            RebuildLocalInventory();
            clientsInventoryInitialized.Add(clientsInventoryToInitialize[i]);
            clientsInventoryToInitialize.RemoveAt(i);
            OnDefaultInventoryAdded?.Invoke();
        }
    }

    private IPlayerController SpawnPlayer(FixedString64Bytes playerID, ulong clientID)
    {
        NetworkInventoryManager.Instance.ServerAddPlayer(playerID);

        return playerInitializer.SpawnPlayer(networkInventoryManager, playerSpawnerNetwork, clientID);
    }

    public void InitializePlayersRPC(FixedString64Bytes[] clientsIDs)
    {
        clientsToCreate.AddRange(clientsIDs);
    }

    public void InitializeInventoryRPC(FixedString64Bytes clientsIDs)
    {
        clientsInventoryToInitialize.Add(clientsIDs);
    }

    public void RebuildLocalInventory()
    {
        networkInventoryManager.RebuildLocalInventory();
    }
}
