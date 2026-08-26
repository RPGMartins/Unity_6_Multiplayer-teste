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
        AuthenticationManager authenticationManager = FindAnyObjectByType<AuthenticationManager>();
        AuthenticateID = authenticationManager.AuthenticationID;
        ClientID = OwnerClientId;
        playerController.Initialize(AuthenticateID, ClientID, IsOwner);
        PlayerInitializerNetwork.instance.InitializeInventoryRPC(AuthenticateID);
    }
}
