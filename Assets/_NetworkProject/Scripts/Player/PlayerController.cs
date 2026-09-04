using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
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

    public override void OnNetworkSpawn()
    {
        EnableScripts(IsOwner);
    }

    public void Initialize(FixedString64Bytes playerId, ulong clientID)
    {
        playerInput.onSlotPressed += playerLocalInventoryController.EquipSlotWeapon;

        playerMovement.Initialize(playerInput.GetMovement, playerInput.GetMousePosition, playerLocalInventoryController.GetCurrentWeaponID, playerInput.Shooting);
        playerAnimation.Initialize(playerLocalInventoryController);
        playerLocalInventoryController.Initialize(playerId);
        playerInteract.Initialize(playerInput.Interacting, clientID, playerId);
        playerHitbox.Initilize(playerTransform);

        PlayerInitializer.instance.InitializeInventoryRPC(playerId);

        if (NetworkConnectionChecker.CheckConnection() == false)
        {
            EnableScripts(true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void InitializeRPC(FixedString64Bytes playerId, ulong clientID)
    {
        Initialize(playerId, clientID);
    }

    private void EnableScripts(bool value)
    {
        playerInput.enabled = value;
        playerInteract.enabled = value;
        playerAnimation.enabled = value;
        playerMovement.EnableDisableAim(value);
        playerMovement.enabled = value;
        playerLocalInventoryController.enabled = value;
    }
}
