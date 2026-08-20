using System.Collections;
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

    private List<ulong> clientsInitialized = new List<ulong>();
    private List<ulong> clientsToInitialize = new List<ulong>();

    private void Awake()
    {
        instance = this;

        if (NetworkConnectionChecker.CheckConnection() == true)
        {
            return;
        }

        if (clientsToInitialize.Count <= 0)
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

        List<IPlayerController> playerControllers = new List<IPlayerController>();

        for (int i = clientsToInitialize.Count - 1; i >= 0; i--)
        {
            IPlayerController playerController = SpawnPlayer(clientsToInitialize[i]);

            if (playerController != null)
            {
                playerControllers.Add(playerController);
                clientsInitialized.Add(clientsToInitialize[i]);
                clientsToInitialize.RemoveAt(i);
            }
        }

        if (playerControllers.Count > 0)
        {
            networkInventoryManager.RebuildLocalInventory();
            StartCoroutine(RebuildPlayerInventory(playerControllers));
        }
    }

    private IEnumerator RebuildPlayerInventory(List<IPlayerController> playerControllers)
    {
        yield return new WaitForSeconds(1f);

        foreach (IPlayerController playerController in playerControllers)
        {
            InitializePlayerRPC();
        }
    }

    public void InitializePlayers(List<ulong> clientsIDs)
    {
        List<ulong> tempClients = new List<ulong>();

        foreach (ulong id in clientsIDs)
        {
            if (clientsInitialized.Contains(id) == false)
            {
                tempClients.Add(id);
            }
        }

        clientsToInitialize.AddRange(tempClients);
    }

    private IPlayerController SpawnPlayer(ulong clientID)
    {
        if (IsServer == false)
        {
            return null;
        }

        LobbyManager lobbyManger = FindAnyObjectByType<LobbyManager>(); //TODO usar singleont
        FixedString64Bytes playerID = new FixedString64Bytes();

        foreach (PlayerData playerData in lobbyManger.GetLobbyPlayers())
        {
            if (playerData.ClientId == clientID)
            {
                playerID = playerData.AuthenticationID;

                break;
            }
        }

        NetworkInventoryManager.Instance.ServerAddPlayer(playerID);

        return playerInitializer.SpawnPlayer(networkInventoryManager, playerSpawnerNetwork, playerID, clientID);
    }

    [Rpc(SendTo.NotMe)]
    private void InitializePlayerRPC()
    {
        networkInventoryManager.RebuildLocalInventory();
    }
}
