using Unity.Netcode;
using UnityEngine;

public class GroundItemSpawnerControllerNetwork : NetworkBehaviour
{
    [SerializeField] private GroundItemSpawnerController groundItemSpawnerController;

    private void Awake()
    {
        if (NetworkConnectionChecker.CheckConnection() == false)
        {
            return;
        }

        if (NetworkManager.Singleton.IsServer == false)
        {
            Destroy(groundItemSpawnerController.gameObject);
        }
    }
}
