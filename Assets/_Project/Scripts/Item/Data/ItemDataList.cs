using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class ItemDataList : MonoBehaviour
{
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    public static ItemDataList Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
    }

    public BaseItem GetItemById(FixedString64Bytes itemId)
    {
        foreach (ItemData item in items)
        {
            if (item.ItemID == itemId)
            {
                return item.ToBaseItem();
            }
        }

        Debug.LogError($"Item with ID {itemId} not found");

        return null;
    }
}
