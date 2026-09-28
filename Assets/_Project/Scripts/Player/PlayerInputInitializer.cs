using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInputInitializer : NetworkBehaviour
{
    [SerializeField] private PlayerInput playerInputPrefab;

    private List<PlayerInput> playerInputList = new List<PlayerInput>();
    private List<FixedString64Bytes> playerToInitializeList = new List<FixedString64Bytes>();

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
            bool initialized = false;

            if (NetworkConnectionChecker.CheckConnection() == true)
            {
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

                initialized = InitializePlayerInputBase(playerController);
            }
            else
            {
                PlayerController playerController = PlayerInitializer.instance.GetPlayerControllerByPlayerID(playerToInitializeList[i]);
                initialized = InitializePlayerInputBase(playerController);
            }

            if (initialized == true)
            {
                playerToInitializeList.RemoveAt(i);
            }
        }
    }

    private bool InitializePlayerInputBase(PlayerController playerController)
    {
        PlayerInput playerInput = Instantiate(playerInputPrefab, transform);
        playerInput.Initialize();
        playerController.InitializePlayerInput(playerInput);
        playerInputList.Add(playerInput);

        return true;
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
