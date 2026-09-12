using System;
using Unity.Netcode;
using UnityEngine;

public struct ProjectileSpawnData : INetworkSerializable, IEquatable<ProjectileSpawnData>
{
    private Vector3 origin;
    private Vector3 direction;
    private float velocity;
    private int damage;

    public Vector3 Origin => origin;
    public Vector3 Direction => direction;
    public float Velocity => velocity;
    public int Damage => damage;

    public ProjectileSpawnData(Vector3 origin, Vector3 direction, float velocity, int damage)
    {
        this.origin = origin;
        this.direction = direction;
        this.velocity = velocity;
        this.damage = damage;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref origin);
        serializer.SerializeValue(ref direction);
        serializer.SerializeValue(ref velocity);
        serializer.SerializeValue(ref damage);
    }

    public bool Equals(ProjectileSpawnData other)
    {
        return origin == other.origin && direction == other.direction && velocity == other.velocity && damage == other.damage;
    }
}
