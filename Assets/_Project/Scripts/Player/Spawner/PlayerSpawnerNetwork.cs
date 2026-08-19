using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnerNetwork : MonoBehaviour, IPlayerSpawner
{
    [SerializeField] private GameObject playerPrefabNetwork;
    [SerializeField] private PlayerSpawnerController playerSpawnerController;

    public IPlayerController SpawnPlayer(ulong clientID)
    {
        IPlayerController playerController = playerSpawnerController.SpawnPlayer(playerPrefabNetwork);
        NetworkObject instanceNetworkObject = playerController.Transform.GetComponentInParent<NetworkObject>();
        instanceNetworkObject.SpawnAsPlayerObject(clientID, true);

        return playerController;
    }
}
