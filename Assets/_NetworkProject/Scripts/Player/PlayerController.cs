using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPlayerController
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerLocalInventoryController playerLocalInventoryController;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerInteract playerInteract;

    public PlayerMovement PlayerMovement => playerMovement;
    public PlayerLocalInventoryController PlayerLocalInventoryController => playerLocalInventoryController;
    public PlayerAnimation PlayerAnimation => playerAnimation;
    public PlayerInput PlayerInput => playerInput;
    public PlayerInteract PlayerInteract => playerInteract;

    private void Awake()
    {
        playerInput.onSlotPressed += playerLocalInventoryController.EquipSlotWeapon;

        playerMovement.Initialize(playerInput.GetMovement, playerInput.GetMousePosition);
        playerAnimation.Initialize(playerLocalInventoryController);
    }

    public void Initialize(FixedString64Bytes authenticateID, ulong clientID)
    {
        AuthenticateID = authenticateID;
        playerLocalInventoryController.Initialize(authenticateID);
        playerInteract.Initialize(playerInput.Interacting, clientID, authenticateID);
    }

    public Transform Transform => transform;
    public FixedString64Bytes AuthenticateID { get; set; }
}
