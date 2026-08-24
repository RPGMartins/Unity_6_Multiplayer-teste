using System;
using Unity.Collections;

public interface IInventoryManager
{
    public event Action<FixedString64Bytes> OnInventoryChanged;

    public bool ServerAddItem(BaseItem baseItem, FixedString64Bytes playerID);
}
