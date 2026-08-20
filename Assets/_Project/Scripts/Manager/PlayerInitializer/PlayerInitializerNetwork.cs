using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInitializerNetwork : NetworkBehaviour
{
    public static PlayerInitializerNetwork instance;
    [SerializeField] private PlayerInitializerSingleplayer playerInitializerSingleplayer;
    [SerializeField] private PlayerSpawnerNetwork playerSpawnerNetwork;
    [SerializeField] private NetworkInventoryManager networkInventoryManager;

    [SerializeField] private PlayerInitializer playerInitializer;

    private List<ulong> clientsCreated = new List<ulong>();
    private List<ulong> clientsToCreate = new List<ulong>();

    private List<ulong> clientsInventoryInitialized = new List<ulong>();
    private List<ulong> clientsInventoryToInitialized = new List<ulong>();

    private void Awake()
    {
        instance = this;

        if (NetworkConnectionChecker.CheckConnection() == true)
        {
            return;
        }

        if (clientsToCreate.Count <= 0)
        {
            playerInitializerSingleplayer.SpawnPlayer(0);
        }
    }

    private void Update()
    {
        if (IsServer == false)
        {
            return;
        }

        for (int i = clientsInventoryToInitialized.Count - 1; i >= 0; i--)
        {
            networkInventoryManager.RebuildLocalInventory();
            ClientRebuildLocalInventoryRPC();

            clientsInventoryInitialized.Add(clientsInventoryToInitialized[i]);
            clientsInventoryToInitialized.RemoveAt(i);
        }

        for (int i = clientsToCreate.Count - 1; i >= 0; i--)
        {
            LobbyManager lobbyManger = FindAnyObjectByType<LobbyManager>(); //TODO usar singleont
            FixedString64Bytes playerID = new FixedString64Bytes();

            foreach (PlayerData playerData in lobbyManger.GetLobbyPlayers())
            {
                if (playerData.ClientId == clientsToCreate[i])
                {
                    playerID = playerData.AuthenticationID;

                    break;
                }
            }

            SpawnPlayer(playerID, clientsToCreate[i]);
            playerInitializer.InitializeDefaultInventoryToPlayer(networkInventoryManager, playerID);

            clientsCreated.Add(clientsToCreate[i]);
            clientsToCreate.RemoveAt(i);
        }
    }

    public void InitializePlayers(List<ulong> clientsIDs)
    {
        List<ulong> tempClients = new List<ulong>();

        foreach (ulong id in clientsIDs)
        {
            if (clientsCreated.Contains(id) == false)
            {
                tempClients.Add(id);
            }
        }

        clientsToCreate.AddRange(tempClients);
    }

    private IPlayerController SpawnPlayer(FixedString64Bytes playerID, ulong clientID)
    {
        if (IsServer == false)
        {
            return null;
        }

        NetworkInventoryManager.Instance.ServerAddPlayer(playerID);

        return playerInitializer.SpawnPlayer(networkInventoryManager, playerSpawnerNetwork, playerID, clientID);
    }

    [Rpc(SendTo.NotMe)]
    private void ClientRebuildLocalInventoryRPC()
    {
        networkInventoryManager.RebuildLocalInventory();
    }

    [Rpc(SendTo.Server)]
    public void InitializeInventoryRPC(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        clientsInventoryToInitialized.Add(clientId);
    }
}
