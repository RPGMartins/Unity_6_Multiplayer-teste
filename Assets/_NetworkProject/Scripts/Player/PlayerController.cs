using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private PlayerHitbox playerHitbox;
    [SerializeField] private PlayerCombat playerCombat;

    private PlayerInput playerInput;

    public PlayerMovement PlayerMovement => playerMovement;
    public PlayerAnimation PlayerAnimation => playerAnimation;
    public PlayerInteract PlayerInteract => playerInteract;

    public Transform Transform => transform;
    public FixedString64Bytes AuthenticateID { get; set; }
    public ulong ClientID { get; set; }

    private Action<WeaponItem> onPlayerEquippedWeaponChanged;

    public override void OnNetworkSpawn()
    {
        EnableScripts(IsOwner);
    }

    public override void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onPlayerEquippedSlotChanged -= PlayerEquippedSlotChanged;
        }

        if (playerInput != null)
        {
            playerInput.onSlotPressed -= CallEquipSlotWeapon;
        }

        onPlayerEquippedWeaponChanged -= playerAnimation.ChangeWeaponAnimation;

        base.OnDestroy();
    }

    public void Initialize(FixedString64Bytes playerId, ulong clientID)
    {
        AuthenticateID = playerId;
        ClientID = clientID;

        playerHitbox.Initilize(playerTransform);
        playerInteract.Initialize(clientID, playerId);

        PlayerInitializer.instance.PlayerInitializedCallback(playerId);

        if (NetworkConnectionChecker.CheckConnection() == false)
        {
            EnableScripts(true);
        }
    }

    public void InitializePlayerInput(PlayerInput playerInput)
    {
        this.playerInput = playerInput;

        InventoryManager.Instance.onPlayerEquippedSlotChanged += PlayerEquippedSlotChanged;
        playerInput.onSlotPressed += CallEquipSlotWeapon;
        onPlayerEquippedWeaponChanged += playerAnimation.ChangeWeaponAnimation;

        playerMovement.Initialize(playerInput.GetMovement, playerCombat.GetAngleToRotate);
        playerCombat.Initialize(playerInput.GetMousePosition, playerInput.Shooting);
        playerInteract.InitializeInput(playerInput.Interacting);
    }

    private void CallEquipSlotWeapon(int slot)
    {
        BaseItem baseItem = InventoryManager.Instance.GetPlayerItemBySlot(AuthenticateID, slot);

        if (baseItem == null)
        {
            return;
        }

        InventoryData inventoryData = new InventoryData(AuthenticateID, baseItem.ItemID, true);

        if (NetworkConnectionChecker.CheckConnection() == false)
        {
            InventoryManager.Instance.EquipItem(inventoryData);
        }
        else
        {
            NetworkInventoryManager.Instance.EquipItem(inventoryData);
        }
    }

    private void PlayerEquippedSlotChanged(FixedString64Bytes playerID, WeaponItem weaponItem)
    {
        if (AuthenticateID != playerID)
        {
            return;
        }

        onPlayerEquippedWeaponChanged?.Invoke(weaponItem);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void InitializeRPC(FixedString64Bytes playerId, ulong clientID)
    {
        Initialize(playerId, clientID);
    }

    private void EnableScripts(bool value)
    {
        playerInteract.enabled = value;
        playerAnimation.enabled = value;
        playerMovement.enabled = value;
        playerCombat.enabled = value;
        playerCombat.EnableDisableAim(value);
    }
}
