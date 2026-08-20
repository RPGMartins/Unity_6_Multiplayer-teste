using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkController : NetworkBehaviour, IPlayerController
{
    [SerializeField] private PlayerController playerController;
    public Transform Transform => transform;
    public PlayerLocalInventoryController PlayerLocalInventoryController => playerController.PlayerLocalInventoryController;

    public FixedString64Bytes AuthenticateID { get; set; }
    public ulong ClientID { get; set; }

    public override void OnNetworkSpawn()
    {
        if (IsOwner == false)
        {
            return;
        }

        AuthenticationManager authenticationManager = FindAnyObjectByType<AuthenticationManager>();
        AuthenticateID = authenticationManager.AuthenticationID;
        ClientID = OwnerClientId;

        if (IsOwner == false)
        {
            Destroy(this);
            Destroy(playerController.PlayerMovement.Target.gameObject);
            Destroy(playerController.PlayerMovement);
            Destroy(playerController.PlayerLocalInventoryController);
            Destroy(playerController.PlayerAnimation);
            Destroy(playerController.PlayerInput);
            Destroy(playerController);
        }

        playerController.Initialize(AuthenticateID, ClientID);

        NetworkInventoryManager.Instance.RebuildLocalInventory();
    }
}
