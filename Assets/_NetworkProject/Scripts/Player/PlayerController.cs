using Unity.Collections;
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
        if (NetworkConnectionChecker.CheckConnection() == false)
        {
            Initialize(PlayerInitializer.DEFAULT_PLAYER_ID, 0);
        }
    }

    public void Initialize(FixedString64Bytes playerId, ulong clientID, bool enableScripts = true)
    {
        playerInput.onSlotPressed += playerLocalInventoryController.EquipSlotWeapon;

        playerMovement.Initialize(playerInput.GetMovement, playerInput.GetMousePosition, playerInput.Shooting);
        playerAnimation.Initialize(playerLocalInventoryController);

        playerLocalInventoryController.Initialize(playerId);
        playerInteract.Initialize(playerInput.Interacting, clientID, playerId);

        DisableOtherPlayersScripts(enableScripts);
    }

    private void DisableOtherPlayersScripts(bool enable)
    {
        PlayerMovement.EnableDisableAim(enable);

        PlayerAnimation.enabled = enable;
        playerInput.enabled = enable;
        playerInteract.enabled = enable;
        playerLocalInventoryController.enabled = enable;
        playerMovement.enabled = enable;
    }

    public Transform Transform => transform;
    public FixedString64Bytes AuthenticateID { get; set; }
    public ulong ClientID { get; set; }
}
