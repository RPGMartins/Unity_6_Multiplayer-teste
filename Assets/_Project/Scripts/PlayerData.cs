using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    [SerializeField] private bool isReady;
    public FixedString64Bytes Name;
    public FixedString64Bytes AuthenticationId;

    public bool IsReady => isReady;

    public PlayerData(PlayerData playerData) : this(playerData.AuthenticationId, playerData.Name)
    {
    }

    public PlayerData(FixedString64Bytes authenticationId, FixedString64Bytes playerName)
    {
        AuthenticationId = authenticationId;
        Name = playerName;
        isReady = false;
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
        serializer.SerializeValue(ref AuthenticationId);
    }

    public bool Equals(PlayerData other)
    {
        return isReady == other.isReady &&
               Name.Equals(other.Name) &&
               AuthenticationId.Equals(other.AuthenticationId);
    }

    public override bool Equals(object obj)
    {
        return obj is PlayerData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(isReady, Name, AuthenticationId);
    }

    private static FixedString64Bytes ToFixedString(string value)
    {
        FixedString64Bytes result = default;

        // Evita null e também evita erro caso passe do limite.
        result.CopyFromTruncated(value ?? string.Empty);

        return result;
    }
}
