using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private readonly List<InventoryItem> items = new List<InventoryItem>();
    [SerializeField] private InventoryItem currentItem;

    public event Action OnInventoryChanged;
    public event Action<InventoryItem> onEquippedSlotChanged;

    public void AddItem(InventoryItem item)
    {
        items.Add(item);

        OnInventoryChanged?.Invoke();
    }

    public void EquipSlot(int slot)
    {
        if (slot < 0 || slot >= items.Count || items[slot] == currentItem)
        {
            return;
        }

        currentItem = items[slot];
        onEquippedSlotChanged?.Invoke(currentItem);
    }
}
