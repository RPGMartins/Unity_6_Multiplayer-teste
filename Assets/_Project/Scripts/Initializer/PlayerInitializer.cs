using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInitializer : NetworkBehaviour
{
    public static string defaultFlashLightID = "FlashLight";
    public static string defaultPistolID = "Pistol";

    public static PlayerInitializer instance;

    public event Action<FixedString64Bytes> OnPlayerInitialized;

    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private InventoryManager inventoryManager;

    private List<FixedString64Bytes> clientsToSpawn = new List<FixedString64Bytes>();
    private List<PlayerController> playerCreatedList = new List<PlayerController>();

    private SessionManager sessionManager;

    public List<PlayerController> PlayerCreatedList => playerCreatedList;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        sessionManager = SessionManager.Instance;
    }

    private void Update()
    {
        if (SessionManager.Instance.CurrentSession.IsOnline == false)
        {
            Spawn(false);
        }
        else if (SessionManager.Instance.CurrentSession.HasAuthority == true)
        {
            Spawn(true);
        }
    }

    private void Spawn(bool network)
    {
        for (int i = clientsToSpawn.Count - 1; i >= 0; i--)
        {
            bool hasPlayer = sessionManager.CurrentSession.GetPlayerDataByPlayerID(clientsToSpawn[i], out PlayerData playerData);

            if (hasPlayer == false)
            {
                throw new Exception("Player data not found");
            }

            ulong clientID = playerData.ClientId;
            FixedString64Bytes playerID = playerData.AuthenticationID;

            inventoryManager.AddPlayer(playerID);
            PlayerController playerController = SpawnPlayer(clientID, i, network);
            playerCreatedList.Add(playerController);
            playerController.Initialize(playerID, clientID);
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
