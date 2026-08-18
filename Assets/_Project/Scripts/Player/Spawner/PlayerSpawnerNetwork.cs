using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnerNetwork : MonoBehaviour, IPlayerSpawner
{
    [SerializeField] private GameObject playerPrefabNetwork;
    [SerializeField] private PlayerSpawnerController playerSpawnerController;

    public PlayerController SpawnPlayer(ulong clientID)
    {
        PlayerController playerController = playerSpawnerController.SpawnPlayer(playerPrefabNetwork);
        NetworkObject instanceNetworkObject = playerController.GetComponentInParent<NetworkObject>();
        instanceNetworkObject.SpawnAsPlayerObject(clientID, true);

        return playerController;
    }
}
