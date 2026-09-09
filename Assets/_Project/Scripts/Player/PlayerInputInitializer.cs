using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInputInitializer : NetworkBehaviour
{
    public static PlayerInputInitializer instance;

    [SerializeField] private PlayerInput playerInputPrefab;

    private List<PlayerInput> playerInputList = new List<PlayerInput>();
    private List<FixedString64Bytes> playerToInitializeList = new List<FixedString64Bytes>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        PlayerInitializer.instance.OnPlayerInitialized += ReceivePlayerID;
    }

    public override void OnDestroy()
    {
        if (PlayerInitializer.instance != null)
        {
            PlayerInitializer.instance.OnPlayerInitialized -= ReceivePlayerID;
        }
    }

    private void Update()
    {
        for (int i = playerToInitializeList.Count - 1; i >= 0; i--)
        {
            if (NetworkConnectionChecker.CheckConnection() == true)
            {
                InitializePlayerInputNetwork(i);
            }
            else
            {
                InitializePlayerInput(i);
            }
        }
    }

    private void InitializePlayerInputNetwork(int index)
    {
        FixedString64Bytes playerID = playerToInitializeList[index];

        NetworkObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;

        if (playerObject == null)
        {
            return;
        }

        PlayerController playerController = playerObject.GetComponentInChildren<PlayerController>();

        if (playerController == null)
        {
            return;
        }

        if (playerController.AuthenticateID.Value != playerID)
        {
            playerToInitializeList.RemoveAt(index);

            return;
        }

        PlayerInput playerInput = Instantiate(playerInputPrefab, transform);
        playerInput.Initialize(playerID);
        playerController.InitializePlayerInput(playerInput);
        playerInputList.Add(playerInput);
        playerToInitializeList.RemoveAt(index);
    }

    private void InitializePlayerInput(int index)
    {
        FixedString64Bytes playerID = playerToInitializeList[index];

        PlayerController playerController = PlayerInitializer.instance.GetPlayerControllerByPlayerID(playerID);

        if (playerController == null)
        {
            return;
        }

        PlayerInput playerInput = Instantiate(playerInputPrefab, transform);
        playerInput.Initialize(playerID);
        playerController.InitializePlayerInput(playerInput);
        playerInputList.Add(playerInput);
        playerToInitializeList.RemoveAt(index);
    }

    private void ReceivePlayerID(FixedString64Bytes playerID)
    {
        playerToInitializeList.Add(playerID);

        if (NetworkConnectionChecker.CheckConnection() == true)
        {
            ReceivePlayerIDRPC(playerID);
        }
    }

    [Rpc(SendTo.NotMe)]
    private void ReceivePlayerIDRPC(FixedString64Bytes playerID)
    {
        playerToInitializeList.Add(playerID);
    }
}
