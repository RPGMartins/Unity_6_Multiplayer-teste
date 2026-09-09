using Unity.Collections;
using UnityEngine;

public class LocalInteractHandler : MonoBehaviour, InterfaceInteractHandler
{
    FixedString64Bytes playerID;

    public void Initialize(FixedString64Bytes playerID, ulong clientID)
    {
        this.playerID = playerID;
    }

    public void Interact(IInteractable interactable)
    {
        if (interactable is GroundItem groundItem)
        {
            if (AddItemToInventory(groundItem) == true)
            {
                GetGroundItemFeedBack(true);

                GroundItemSpawnerController.instance.DestroyItem(groundItem.ItemIdentificationID, false);
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
        InventoryManager.Instance.ServerAddItem(groundItem.CurrentItem, playerID);

        return true;
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
