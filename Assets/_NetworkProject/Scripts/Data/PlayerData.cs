using System;
using Unity.Collections;
using Unity.Netcode;

[Serializable]
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    private bool isReady;
    private ulong clientId;
    private bool isHost;
    public FixedString64Bytes name;
    public FixedString64Bytes authenticationID;

    public bool IsReady => isReady;
    public ulong ClientId => clientId;
    public bool IsHost => isHost;
    public FixedString64Bytes Name => name;
    public FixedString64Bytes AuthenticationID => authenticationID;

    public PlayerData(PlayerData playerData) : this(playerData.authenticationID, playerData.clientId, playerData.name, playerData.isHost)
    {
    }

    public PlayerData(FixedString64Bytes authenticationId, ulong clientId, FixedString64Bytes playerName, bool isHost)
    {
        this.authenticationID = authenticationId;
        this.clientId = clientId;
        this.name = playerName;
        this.isReady = false;
        this.isHost = isHost;
    }

    public void SetClientID(ulong clientId)
    {
        this.clientId = clientId;
    }

    public void SetReady(bool ready)
    {
        isReady = ready;
    }

    public void ToggleReady()
    {
        SetReady(!isReady);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref isReady);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref authenticationID);
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref isHost);
    }

    public bool Equals(PlayerData other)
    {
        return isReady == other.isReady &&
               isHost == other.isHost &&
               name.Equals(other.Name) &&
               clientId.Equals(other.clientId) &&
               authenticationID.Equals(other.authenticationID);
    }

    public override bool Equals(object obj)
    {
        return obj is PlayerData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(isReady, name, authenticationID, clientId);
    }

    private static FixedString64Bytes ToFixedString(string value)
    {
        FixedString64Bytes result = default;

        // Evita null e também evita erro caso passe do limite.
        result.CopyFromTruncated(value ?? string.Empty);

        return result;
    }
}
