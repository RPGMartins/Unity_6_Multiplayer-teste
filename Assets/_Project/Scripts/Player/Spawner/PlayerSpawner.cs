using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private NetworkObject playerPrefabNetwork;

    private void Awake()
    {
        if (NetworkConnectionChecker.CheckConnection() == true)
        {
            return;
        }

        SpawnPlayer();
    }

    public void SpawnPlayerPrefab(ulong clientID)
    {
        if (NetworkManager.Singleton.IsServer == false)
        {
            return;
        }

        SpawnPlayerNetwork(clientID);
    }

    private void SpawnPlayer()
    {
        GameObject instance = Instantiate(playerPrefab);
    }

    private void SpawnPlayerNetwork(ulong clientID)
    {
        NetworkObject instance = Instantiate(playerPrefabNetwork);
        NetworkObject instanceNetworkObject = instance.GetComponent<NetworkObject>();
        instanceNetworkObject.SpawnAsPlayerObject(clientID, true);
    }
}
