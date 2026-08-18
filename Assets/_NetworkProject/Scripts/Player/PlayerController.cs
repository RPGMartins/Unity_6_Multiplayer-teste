using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerLocalInventoryController playerLocalInventoryController;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerInteract playerInteract;

    private bool initialized = false;

    public PlayerMovement PlayerMovement => playerMovement;
    public PlayerLocalInventoryController PlayerLocalInventoryController => playerLocalInventoryController;
    public PlayerAnimation PlayerAnimation => playerAnimation;
    public PlayerInput PlayerInput => playerInput;
    public PlayerInteract PlayerInteract => playerInteract;

    private FixedString64Bytes authenticateID;

    public FixedString64Bytes AuthenticateID => authenticateID;

    public void Initialize(FixedString64Bytes authenticateID, ulong clientID)
    {
        if (initialized == true)
        {
            Debug.LogWarning("PlayerController already initialized");
        }

        this.authenticateID = authenticateID;

        playerInput.onSlotPressed += playerLocalInventoryController.EquipSlotWeapon;

        playerMovement.Initialize(playerInput.GetMovement, playerInput.GetMousePosition);
        playerAnimation.Initialize(playerLocalInventoryController);
        playerInteract.Initialize(playerInput.Interacting, clientID, authenticateID);

        playerLocalInventoryController.Initialize(authenticateID);

        initialized = true;
    }
}
