using System;
using Unity.Collections;
using UnityEngine;

public interface IInventoryManager
{
    public event Action<FixedString64Bytes> OnInventoryChanged;

    public void AddPlayer(FixedString64Bytes playerID);
    public bool AddItemToPlayer(BaseItem baseItem, FixedString64Bytes playerID);
}
