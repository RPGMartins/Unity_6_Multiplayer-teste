using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkController : NetworkBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;

    public override void OnNetworkSpawn()
    {
        if (IsOwner == false)
        {
            Destroy(this);
            Destroy(playerMovement);
        }
    }
}
