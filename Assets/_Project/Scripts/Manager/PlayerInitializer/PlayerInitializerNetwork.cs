using System;
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

    private List<FixedString64Bytes> clientsCreated = new List<FixedString64Bytes>();
    private List<FixedString64Bytes> clientsToCreate = new List<FixedString64Bytes>();

    private List<FixedString64Bytes> clientsInventoryInitialized = new List<FixedString64Bytes>();
    private List<FixedString64Bytes> clientsInventoryToInitialized = new List<FixedString64Bytes>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
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
            ulong clientID = 0;

            List<PlayerData> lobbyPlayers = lobbyManger.GetLobbyPlayers();

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
            playerInitializer.InitializeDefaultInventoryToPlayer(networkInventoryManager, clientsToCreate[i]);

            clientsCreated.Add(clientsToCreate[i]);
            clientsToCreate.RemoveAt(i);
        }
    }

    public void InitializePlayers(List<ulong> clientsIDs)
    {
        if (IsServer == false)
        {
            return;
        }

        foreach (ulong id in clientsIDs)
        {
            RequestClientAuthenticatePlayerIdRPC(RpcTarget.Single(id, RpcTargetUse.Temp));
        }
    }

    private IPlayerController SpawnPlayer(FixedString64Bytes playerID, ulong clientID)
    {
        if (IsServer == false)
        {
            return null;
        }

        NetworkInventoryManager.Instance.ServerAddPlayer(playerID);

        return playerInitializer.SpawnPlayer(networkInventoryManager, playerSpawnerNetwork, clientID);
    }

    [Rpc(SendTo.NotMe)]
    private void ClientRebuildLocalInventoryRPC()
    {
        networkInventoryManager.RebuildLocalInventory();
    }

    [Rpc(SendTo.Server)]
    public void InitializeInventoryRPC(FixedString64Bytes playerID)
    {
        clientsInventoryToInitialized.Add(playerID);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void RequestClientAuthenticatePlayerIdRPC(RpcParams rpcParams = default)
    {
        AuthenticationManager authenticationManager = FindAnyObjectByType<AuthenticationManager>(); //TODO usar singleton
        ReceivePlayerIdRPC(authenticationManager.AuthenticationID);
    }

    [Rpc(SendTo.Server)]
    private void ReceivePlayerIdRPC(FixedString64Bytes playerID)
    {
        clientsToCreate.Add(playerID);
    }
}
