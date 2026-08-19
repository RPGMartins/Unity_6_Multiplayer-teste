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

    private bool isConnected = false;

    private void Awake()
    {
        instance = this;

        if (NetworkConnectionChecker.CheckConnection() == true)
        {
            isConnected = true;
        }
    }

    private void Start()
    {
        if (isConnected == false)
        {
            playerInitializerSingleplayer.SpawnPlayer(0);
        }
    }

    public void FinishInitialization()
    {
        InventoryManager.Instance.FinishInitialization();
    }

    public void SpawnPlayer(ulong clientID)
    {
        if (IsServer == false)
        {
            return;
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
        IPlayerController playerController = playerInitializer.SpawnPlayer(playerSpawnerNetwork, playerID, clientID);
        InitializePlayerRPC(playerID, clientID);
        playerInitializer.InitializePlayerInventory(networkInventoryManager, playerController, playerID);
    }

    [Rpc(SendTo.NotMe)]
    private void InitializePlayerRPC(FixedString64Bytes playerID, ulong clientID)
    {
        playerInitializer.SearchAndInitializePlayer(playerID, clientID);
    }
}
