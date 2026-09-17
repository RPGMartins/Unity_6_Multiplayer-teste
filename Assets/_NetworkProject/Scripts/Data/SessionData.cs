using System;
using Unity.Collections;
using Unity.Netcode;

[Serializable]
public struct SessionData : INetworkSerializable, IEquatable<SessionData>
{
    private int maxPlayers;
    private bool isPrivate;
    private FixedString64Bytes lobbyName;
    private FixedString64Bytes lobbyCode;

    public int MaxPlayers => maxPlayers;
    public bool IsPrivate => isPrivate;
    public FixedString64Bytes LobbyName => lobbyName;
    public FixedString64Bytes LobbyCode => lobbyCode;

    public SessionData(int maxPlayers, bool isPrivate, FixedString64Bytes lobbyName, FixedString64Bytes lobbyCode)
    {
        this.lobbyCode = lobbyCode;
        this.maxPlayers = maxPlayers;
        this.isPrivate = isPrivate;
        this.lobbyName = lobbyName;
    }

    public SessionData(SessionData sessionData) : this(sessionData.maxPlayers, sessionData.isPrivate, sessionData.lobbyName, sessionData.lobbyCode)
    {
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref maxPlayers);
        serializer.SerializeValue(ref isPrivate);
        serializer.SerializeValue(ref lobbyName);
        serializer.SerializeValue(ref lobbyCode);
    }

    public bool Equals(SessionData other)
    {
        return maxPlayers == other.maxPlayers &&
               isPrivate == other.isPrivate &&
               lobbyName.Equals(other.lobbyName) &&
               lobbyCode.Equals(other.lobbyCode);
    }

    public override bool Equals(object obj)
    {
        return obj is SessionData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(maxPlayers, isPrivate, lobbyName, lobbyCode);
    }
}
