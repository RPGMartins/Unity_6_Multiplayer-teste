using System;
using Unity.Collections;

public interface IInventoryManager
{
    public event Action<FixedString64Bytes> OnInventoryChanged;

    public void ServerAddPlayer(FixedString64Bytes playerID);
    public bool ServerAddItem(BaseItem baseItem, FixedString64Bytes playerID);
}
