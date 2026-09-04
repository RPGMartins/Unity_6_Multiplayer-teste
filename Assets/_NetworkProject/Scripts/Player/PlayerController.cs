using System;
using Unity.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPlayerController
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerLocalInventoryController playerLocalInventoryController;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private PlayerHitbox playerHitbox;

    public PlayerMovement PlayerMovement => playerMovement;
    public PlayerLocalInventoryController PlayerLocalInventoryController => playerLocalInventoryController;
    public PlayerAnimation PlayerAnimation => playerAnimation;
    public PlayerInput PlayerInput => playerInput;
    public PlayerInteract PlayerInteract => playerInteract;

    public Transform Transform => transform;
    public FixedString64Bytes AuthenticateID { get; set; }
    public ulong ClientID { get; set; }

    public void Initialize(FixedString64Bytes playerId, ulong clientID, Action callback)
    {
        playerInput.onSlotPressed += playerLocalInventoryController.EquipSlotWeapon;

        playerMovement.Initialize(playerInput.GetMovement, playerInput.GetMousePosition, playerLocalInventoryController.GetCurrentWeaponID, playerInput.Shooting);
        playerAnimation.Initialize(playerLocalInventoryController);
        playerLocalInventoryController.Initialize(playerId);
        playerInteract.Initialize(playerInput.Interacting, clientID, playerId);
        playerHitbox.Initilize(playerTransform);
        callback.Invoke();
    }
}
