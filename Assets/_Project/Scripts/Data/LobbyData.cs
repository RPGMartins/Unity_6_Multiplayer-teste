using System;
using Unity.Collections;
using Unity.Netcode;

[Serializable]
public struct LobbyData : INetworkSerializable, IEquatable<LobbyData>
{
    public int MaxPlayers;
    public bool IsPrivate;
    public FixedString64Bytes LobbyName;
    public FixedString64Bytes LobbyCode;

    public LobbyData(int maxPlayers, bool isPrivate, FixedString64Bytes lobbyName, FixedString64Bytes lobbyCode)
    {
        LobbyCode = lobbyCode;
        MaxPlayers = maxPlayers;
        IsPrivate = isPrivate;
        LobbyName = lobbyName;
    }

    public LobbyData(LobbyData lobbyData) : this(lobbyData.MaxPlayers, lobbyData.IsPrivate, lobbyData.LobbyName, lobbyData.LobbyCode)
    {
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref MaxPlayers);
        serializer.SerializeValue(ref IsPrivate);
        serializer.SerializeValue(ref LobbyName);
        serializer.SerializeValue(ref LobbyCode);
    }

    public bool Equals(LobbyData other)
    {
        return MaxPlayers == other.MaxPlayers &&
               IsPrivate == other.IsPrivate &&
               LobbyName.Equals(other.LobbyName) &&
               LobbyCode.Equals(other.LobbyCode);
    }

    public override bool Equals(object obj)
    {
        return obj is LobbyData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            MaxPlayers,
            IsPrivate,
            LobbyName, LobbyCode);
    }
}
