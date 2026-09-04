using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct InventoryData : INetworkSerializable, IEquatable<InventoryData>
{
    private FixedString64Bytes playerID;
    private FixedString64Bytes itemID;

    public FixedString64Bytes PlayerID => playerID;
    public FixedString64Bytes ItemID => itemID;

    public InventoryData(FixedString64Bytes playerID, FixedString64Bytes itemID)
    {
        this.playerID = playerID;
        this.itemID = itemID;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerID);
        serializer.SerializeValue(ref itemID);
    }

    public bool Equals(InventoryData other)
    {
        return playerID.Equals(other.playerID) && itemID.Equals(other.itemID);
    }
}

[Serializable]
public struct SerializedInventoryData
{
    [SerializeField] private string playerID;
    [SerializeField] private string itemID;

    public SerializedInventoryData(InventoryData inventoryData)
    {
        this.playerID = inventoryData.PlayerID.Value;
        this.itemID = inventoryData.ItemID.Value;
    }

    public string PlayerID => playerID;
    public string ItemID => itemID;
}
