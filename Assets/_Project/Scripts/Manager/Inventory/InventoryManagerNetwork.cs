using System;
using Unity.Collections;
using Unity.Netcode;

public class InventoryManagerNetwork : NetworkBehaviour, IInventoryManager
{
    public static InventoryManagerNetwork Instance { get; private set; }

    public event Action<FixedString64Bytes> OnInventoryChanged;

    private void Awake()
    {
        Instance = this;
    }

    public void AddPlayer(FixedString64Bytes playerID)
    {
        if (IsServer == false)
        {
            return;
        }

        InventoryManager.Instance.AddPlayer(playerID);
        AddPlayerRPC(playerID);
    }

    public bool AddItemToPlayer(BaseItem baseItem, FixedString64Bytes playerID)
    {
        if (IsServer == false)
        {
            return false;
        }

        InventoryManager.Instance.AddItemToPlayer(baseItem, playerID);
        AddItemRPC(baseItem.ItemID, playerID);

        return true;
    }

    [Rpc(SendTo.NotMe)]
    private void AddItemRPC(FixedString64Bytes itemId, FixedString64Bytes playerID)
    {
        BaseItem item = ItemDataList.Instance.GetItemById(itemId);
        InventoryManager.Instance.AddItemToPlayer(item, playerID);
    }

    [Rpc(SendTo.NotMe)]
    private void AddPlayerRPC(FixedString64Bytes playerID)
    {
        InventoryManager.Instance.AddPlayer(playerID);
    }
}
