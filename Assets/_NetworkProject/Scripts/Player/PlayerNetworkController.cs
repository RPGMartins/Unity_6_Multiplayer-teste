using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkController : NetworkBehaviour
{
    [SerializeField] private PlayerController playerController;

    public override void OnNetworkSpawn()
    {
        if (IsOwner == false)
        {
            Destroy(this);
            Destroy(playerController.PlayerMovement.Target.gameObject);
            Destroy(playerController.PlayerMovement);
            Destroy(playerController.PlayerInventory);
            Destroy(playerController.PlayerAnimation);
            Destroy(playerController.PlayerInput);
            Destroy(playerController);
        }
    }
}
