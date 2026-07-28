using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    [SerializeField] private bool _isReady;
    public FixedString64Bytes Name;
    public FixedString64Bytes AuthenticationId;

    public bool IsReady => _isReady;

    public PlayerData(PlayerData playerData) : this(playerData.AuthenticationId,playerData.Name)
    {
        
    }
    public PlayerData(FixedString64Bytes authenticationId, FixedString64Bytes playerName)
    {
        AuthenticationId = authenticationId;
        Name = playerName;
        _isReady = false;
    }

    public void SetReady(bool ready)
    {
        _isReady = ready;
    }

    public void ToggleReady()
    {
        _isReady = !_isReady;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _isReady);
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref AuthenticationId);
    }

    public bool Equals(PlayerData other)
    {
        return _isReady == other._isReady &&
               Name.Equals(other.Name) &&
               AuthenticationId.Equals(other.AuthenticationId);
    }

    public override bool Equals(object obj)
    {
        return obj is PlayerData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_isReady, Name, AuthenticationId);
    }
}
