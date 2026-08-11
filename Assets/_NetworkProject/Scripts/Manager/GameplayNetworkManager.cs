using Unity.Netcode;
using UnityEngine;

public class GameplayNetworkManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    public void SpawnPlayerPrefab(ulong clientID)
    {
        if (NetworkManager.Singleton.IsServer == false)
        {
            return;
        }

        NetworkObject instance = Instantiate(playerPrefab);
        NetworkObject instanceNetworkObject = instance.GetComponent<NetworkObject>();
        instanceNetworkObject.SpawnWithOwnership(clientID, true);
    }
}
