using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerInteract playerInteract;
    public PlayerMovement PlayerMovement => playerMovement;
    public PlayerInventory PlayerInventory => playerInventory;
    public PlayerAnimation PlayerAnimation => playerAnimation;
    public PlayerInput PlayerInput => playerInput;
    public PlayerInteract PlayerInteract => playerInteract;

    private void Awake()
    {
        playerInput.onSlotPressed += playerInventory.EquipSlotWeapon;

        playerMovement.Initialize(playerInput.GetMovement, playerInput.GetMousePosition);
        playerAnimation.Initialize(playerInventory);
        playerInteract.Initialize(playerInventory, playerInput.Interacting);
    }
}
