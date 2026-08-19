using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkController : NetworkBehaviour, IPlayerController
{
    [SerializeField] private PlayerController playerController;

    private NetworkVariable<FixedString64Bytes> authenticateID = new NetworkVariable<FixedString64Bytes>();
    public Transform Transform => transform;
    public PlayerLocalInventoryController PlayerLocalInventoryController => playerController.PlayerLocalInventoryController;

    public FixedString64Bytes AuthenticateID
    {
        get => authenticateID.Value;
        set => authenticateID.Value = value;
    }

    public void Initialize(FixedString64Bytes authenticateID, ulong clientID)
    {
        if (IsOwner == false)
        {
            Destroy(this);
            Destroy(playerController.PlayerMovement.Target.gameObject);
            Destroy(playerController.PlayerMovement);
            Destroy(playerController.PlayerLocalInventoryController);
            Destroy(playerController.PlayerAnimation);
            Destroy(playerController.PlayerInput);
            Destroy(playerController);

            return;
        }

        AuthenticateID = authenticateID;
        playerController.Initialize(authenticateID, clientID);
    }
}
