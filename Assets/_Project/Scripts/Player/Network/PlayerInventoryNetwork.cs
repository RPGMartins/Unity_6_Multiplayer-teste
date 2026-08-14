using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventoryNetwork : NetworkBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;

    public bool ServerTryAddItemToInventory(BaseItem baseItem)
    {
        if (IsServer == false)
        {
            return false;
        }

        if (playerInventory.TryAddItemToInventory(baseItem) == false)
        {
            return false;
        }

        AddItemResultRpc(baseItem.ItemID, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));

        return true;
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void AddItemResultRpc(FixedString64Bytes itemId, RpcParams rpcParams = default)
    {
        BaseItem item = ItemDataList.Instance.GetItemById(itemId);

        if (IsHost == false) //Host nao duplicar itens
        {
            playerInventory.TryAddItemToInventory(item);
        }
    }
}
