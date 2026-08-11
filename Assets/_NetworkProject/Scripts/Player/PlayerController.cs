using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerInput playerInput;
    public PlayerMovement PlayerMovement => playerMovement;
    public PlayerInventory PlayerInventory => playerInventory;
    public PlayerAnimation PlayerAnimation => playerAnimation;
    public PlayerInput PlayerInput => playerInput;

    private void Awake()
    {
        playerInput.onSlotPressed += playerInventory.EquipSlot;

        playerMovement.Initialize(playerInput.GetMovement, playerInput.GetMousePosition);
        playerAnimation.Initialize(playerInventory);
    }

    private void Start()
    {
        InventoryItem inventoryItemFlashlight = new InventoryItem(WeaponType.Flashlight);
        InventoryItem inventoryItemPistol = new InventoryItem(WeaponType.Pistol);
        InventoryItem inventoryItemShotgun = new InventoryItem(WeaponType.Shotgun);
        InventoryItem inventoryItemRifle = new InventoryItem(WeaponType.Rifle);

        playerInventory.AddItem(inventoryItemFlashlight);
        playerInventory.AddItem(inventoryItemPistol);
        playerInventory.AddItem(inventoryItemShotgun);
        playerInventory.AddItem(inventoryItemRifle);
        playerInventory.EquipSlot(0);
    }
}
