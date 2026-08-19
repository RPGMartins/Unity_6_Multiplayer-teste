using System;
using Unity.Collections;
using Unity.Netcode;

public struct NetworkInventoryItem : INetworkSerializable, IEquatable<NetworkInventoryItem>
{
    private FixedString64Bytes playerID;
    private FixedString64Bytes itemID;

    public FixedString64Bytes PlayerID => playerID;
    public FixedString64Bytes ItemID => itemID;

    public NetworkInventoryItem(FixedString64Bytes playerID, FixedString64Bytes itemID)
    {
        this.playerID = playerID;
        this.itemID = itemID;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerID);
        serializer.SerializeValue(ref itemID);
    }

    public bool Equals(NetworkInventoryItem other)
    {
        return playerID.Equals(other.playerID) && itemID.Equals(other.itemID);
    }
}
