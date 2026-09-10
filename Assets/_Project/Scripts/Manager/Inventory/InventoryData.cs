using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct InventoryData : INetworkSerializable, IEquatable<InventoryData>
{
    private FixedString64Bytes playerID;
    private FixedString64Bytes itemID;
    private bool equipped;

    public FixedString64Bytes PlayerID => playerID;
    public FixedString64Bytes ItemID => itemID;
    public bool Equipped => equipped;

    public InventoryData(FixedString64Bytes playerID, FixedString64Bytes itemID, bool equipped)
    {
        this.playerID = playerID;
        this.itemID = itemID;
        this.equipped = equipped;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerID);
        serializer.SerializeValue(ref itemID);
        serializer.SerializeValue(ref equipped);
    }

    public bool Equals(InventoryData other)
    {
        return playerID.Equals(other.playerID) && itemID.Equals(other.itemID) && equipped.Equals(other.equipped);
    }
}

[Serializable]
public struct SerializedInventoryData
{
    [SerializeField] private string playerID;
    [SerializeField] private string itemID;
    [SerializeField] private bool equipped;

    public SerializedInventoryData(InventoryData inventoryData)
    {
        this.playerID = inventoryData.PlayerID.Value;
        this.itemID = inventoryData.ItemID.Value;
        this.equipped = inventoryData.Equipped;
    }

    public string PlayerID => playerID;
    public string ItemID => itemID;
}
