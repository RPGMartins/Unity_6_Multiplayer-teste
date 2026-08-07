using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    [SerializeField] private bool isReady;
    private ulong clientId;
    public FixedString64Bytes Name;
    public FixedString64Bytes AuthenticationID;

    public bool IsReady => isReady;
    public ulong ClientId => clientId;

    public PlayerData(PlayerData playerData) : this(playerData.AuthenticationID, playerData.clientId, playerData.Name)
    {
    }

    public PlayerData(FixedString64Bytes authenticationId, ulong clientId, FixedString64Bytes playerName)
    {
        AuthenticationID = authenticationId;
        this.clientId = clientId;
        Name = playerName;
        isReady = false;
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
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref AuthenticationID);
        serializer.SerializeValue(ref clientId);
    }

    public bool Equals(PlayerData other)
    {
        return isReady == other.isReady &&
               Name.Equals(other.Name) &&
               clientId.Equals(other.clientId) &&
               AuthenticationID.Equals(other.AuthenticationID);
    }

    public override bool Equals(object obj)
    {
        return obj is PlayerData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(isReady, Name, AuthenticationID, clientId);
    }

    private static FixedString64Bytes ToFixedString(string value)
    {
        FixedString64Bytes result = default;

        // Evita null e também evita erro caso passe do limite.
        result.CopyFromTruncated(value ?? string.Empty);

        return result;
    }
}
