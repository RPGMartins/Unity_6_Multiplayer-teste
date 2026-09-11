using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInitializer : NetworkBehaviour
{
    private static string defaultPlayerID = "DefaultPlayer";
    public static string defaultFlashLightID = "FlashLight";
    public static string defaultPistolID = "Pistol";

    public static PlayerInitializer instance;

    public event Action<FixedString64Bytes> OnPlayerInitialized;

    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private InventoryManager inventoryManager;

    private List<FixedString64Bytes> clientsToSpawn = new List<FixedString64Bytes>();
    private List<PlayerController> playerCreatedList = new List<PlayerController>();

    private LobbyManager lobbyManager;

    public List<PlayerController> PlayerCreatedList => playerCreatedList;

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

        if (clientsToSpawn.Count <= 0)
        {
            PreparePlayersToBeSpawned(new FixedString64Bytes[] { defaultPlayerID });
        }
    }

    private void Update()
    {
        if (NetworkConnectionChecker.CheckIfHasAuthority() == true)
        {
            SpawnOnline();
        }
        else
        {
            SpawnOffline();
        }
    }

    private void SpawnOffline()
    {
        for (int i = clientsToSpawn.Count - 1; i >= 0; i--)
        {
            ulong clientID = 0;
            FixedString64Bytes playerID = clientsToSpawn[i];
            PlayerController playerController = SpawnPlayer(clientID, i, false);
            playerCreatedList.Add(playerController);
            playerController.Initialize(playerID, clientID);
        }
    }

    private void SpawnOnline()
    {
        for (int i = clientsToSpawn.Count - 1; i >= 0; i--)
        {
            ulong clientID = 0;
            FixedString64Bytes playerID = clientsToSpawn[i];
            List<PlayerData> lobbyPlayers = lobbyManager.GetLobbyPlayers();

            foreach (PlayerData playerData in lobbyPlayers)
            {
                if (playerData.AuthenticationID != clientsToSpawn[i])
                {
                    continue;
                }

                clientID = playerData.ClientId;

                break;
            }

            inventoryManager.ServerAddPlayer(clientsToSpawn[i]);
            PlayerController playerController = SpawnPlayer(clientID, i, true);
            playerCreatedList.Add(playerController);
            playerController.InitializeRPC(playerID, clientID);
        }
    }

    private PlayerController SpawnPlayer(ulong clientID, int index, bool network)
    {
        PlayerController playerController = playerSpawner.SpawnPlayer(clientID, network);
        clientsToSpawn.RemoveAt(index);

        return playerController;
    }

    public void PreparePlayersToBeSpawned(FixedString64Bytes[] clientsIDs)
    {
        clientsToSpawn.AddRange(clientsIDs);
    }

    public PlayerController GetPlayerControllerByPlayerID(FixedString64Bytes playerID)
    {
        foreach (PlayerController playerController in PlayerCreatedList)
        {
            if (playerController.AuthenticateID.Value == playerID)
            {
                return playerController;
            }
        }

        return null;
    }

    public void PlayerInitializedCallback(FixedString64Bytes playerID)
    {
        if (NetworkConnectionChecker.CheckConnection() == true)
        {
            if (NetworkConnectionChecker.CheckIfHasAuthority() == true)
            {
                PlayerInitialized(playerID);
            }
        }
        else
        {
            PlayerInitialized(playerID);
        }
    }

    private void PlayerInitialized(FixedString64Bytes playerID)
    {
        OnPlayerInitialized.Invoke(playerID);
    }
}
