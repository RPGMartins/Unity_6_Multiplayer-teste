using UnityEditor;
using UnityEngine;

public class LocalInteractHandler : MonoBehaviour, InterfaceInteractHandler
{
    private PlayerInventory playerInventory;

    public void Initialize(PlayerInventory inventory)
    {
        this.playerInventory = inventory;
    }

    public void Interact(IInteractable interactable)
    {
        if (interactable is GroundItem groundItem)
        {
            if (AddItemToInventory(groundItem) == true)
            {
                GetGroundItemFeedBack(true);
                groundItem.CallDespawn();
            }

            GetGroundItemFeedBack(false);
        }
        else
        {
            interactable.Interact();
        }
    }

    private bool AddItemToInventory(GroundItem groundItem)
    {
        return playerInventory.TryAddItemToInventory(groundItem.CurrentItem);
    }

    public void GetGroundItemFeedBack(bool value)
    {
        if (value == true)
        {
            //som de pegar, effeitos etc,
        }
        else
        {
            //Som de negacao e etc
        }
    }
}
