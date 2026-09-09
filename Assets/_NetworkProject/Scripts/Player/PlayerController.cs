using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerLocalInventoryController playerLocalInventoryController;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private PlayerHitbox playerHitbox;

    public PlayerMovement PlayerMovement => playerMovement;
    public PlayerLocalInventoryController PlayerLocalInventoryController => playerLocalInventoryController;
    public PlayerAnimation PlayerAnimation => playerAnimation;
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
        AuthenticateID = playerId;
        ClientID = clientID;

        playerAnimation.Initialize(playerLocalInventoryController);
        playerLocalInventoryController.Initialize(playerId);
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
        playerInput.onSlotPressed += playerLocalInventoryController.EquipSlotWeapon;
        playerMovement.Initialize(playerInput.GetMovement, playerInput.GetMousePosition, playerLocalInventoryController.GetCurrentWeaponID, playerInput.Shooting);
        playerInteract.InitializeInput(playerInput.Interacting);
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
        playerMovement.EnableDisableAim(value);
        playerMovement.enabled = value;
        playerLocalInventoryController.enabled = value;
    }
}
