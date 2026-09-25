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

    private Action<WeaponTypeEnum> onPlayerEquippedWeaponChanged;

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
        if (SessionManager.Instance.CurrentSession.IsOnline == true)
        {
            if (SessionManager.Instance.CurrentSession.HasAuthority == true)
            {
                InitializeNetworkRPC(playerId, clientID);
            }
        }
        else
        {
            InitializeLocal(playerId, clientID);
        }
    }

    private void InitializeLocal(FixedString64Bytes playerId, ulong clientID)
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

    [Rpc(SendTo.ClientsAndHost)]
    private void InitializeNetworkRPC(FixedString64Bytes playerId, ulong clientID)
    {
        InitializeLocal(playerId, clientID);
    }

    public void InitializePlayerInput(PlayerInput playerInput)
    {
        this.playerInput = playerInput;

        InventoryManager.Instance.onPlayerEquippedSlotChanged += PlayerEquippedSlotChanged;
        playerInput.onSlotPressed += CallEquipSlotWeapon;
        onPlayerEquippedWeaponChanged += playerAnimation.ChangeWeaponAnimation;

        playerMovement.Initialize(playerInput.GetMovement, playerCombat.GetAngleToRotate);
        playerCombat.Initialize(AuthenticateID, playerInput.GetMousePosition, playerInput.Shooting);
        playerInteract.InitializeInput(playerInput.Interacting);
    }

    private void CallEquipSlotWeapon(int slot)
    {
        BaseItem baseItem = InventoryManager.Instance.GetPlayerItemBySlot(AuthenticateID, slot);

        if (baseItem == null)
        {
            return;
        }

        InventoryManager.Instance.TryEquipItem(new InventoryData(AuthenticateID, baseItem.ItemID, true));
    }

    private void PlayerEquippedSlotChanged(FixedString64Bytes playerID, FixedString64Bytes weaponID)
    {
        if (AuthenticateID != playerID)
        {
            return;
        }

        WeaponItem weaponItem = ItemDataList.Instance.GetItemById(weaponID) as WeaponItem;
        onPlayerEquippedWeaponChanged?.Invoke(weaponItem.WeaponTypeEnum);
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
