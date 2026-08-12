using System;

public class GroundItem : Interactable
{
    private BaseItem currentItem;

    public void Initialize(BaseItem item)
    {
        this.currentItem = item;
    }

    public void Interact(PlayerInventory playerInventory)
    {
        playerInventory.AddItemToInventory(currentItem);
        Destroy(gameObject);
    }
}
