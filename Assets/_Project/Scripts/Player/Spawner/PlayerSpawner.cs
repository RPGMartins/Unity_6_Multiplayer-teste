using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerPrefabNetwork;

    public IPlayerController SpawnPlayer(ulong clientID, bool network)
    {
        IPlayerController playerController = null;

        if (network == true)
        {
            playerController = SpawnPrefab(playerPrefabNetwork);
            NetworkObject instanceNetworkObject = playerController.Transform.GetComponentInParent<NetworkObject>();
            instanceNetworkObject.SpawnAsPlayerObject(clientID, true);
        }
        else
        {
            playerController = SpawnPrefab(playerPrefab);
        }

        return playerController;
    }

    private IPlayerController SpawnPrefab(GameObject tempPrefab)
    {
        GameObject playerInstance = Instantiate(tempPrefab);
        IPlayerController playerController = playerInstance.GetComponentInChildren<IPlayerController>();

        return playerController;
    }
}
