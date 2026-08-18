using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInitializerNetwork : NetworkBehaviour
{
    public static PlayerInitializerNetwork instance;
    [SerializeField] private PlayerInitializer playerInitializer;
    [SerializeField] private PlayerSpawnerNetwork playerSpawnerNetwork;
    [SerializeField] private InventoryManagerNetwork inventoryManagerNetwork;

    [SerializeField] private PlayerInitializerUtils playerInitializerUtils;

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
            playerInitializer.SpawnPlayer(0);
        }
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

        playerInitializerUtils.SpawnPlayer(playerSpawnerNetwork, inventoryManagerNetwork, playerID, clientID);
        InitializePlayerRPC(playerID, clientID);
    }

    [Rpc(SendTo.NotMe)]
    private void InitializePlayerRPC(FixedString64Bytes playerID, ulong clientID)
    {
        playerInitializerUtils.SearchAndInitializePlayer(playerID, clientID);
    }
}
