using System;
using UnityEngine;

[Serializable]
public struct ProjectileData
{
    [SerializeField] private string weaponID;
    [SerializeField] private projectileType projectileType;
    [SerializeField] private float velocity;
    [SerializeField] private int damage;

    public string WeaponID => weaponID;
    public projectileType ProjectileType => projectileType;
    public float Velocity => velocity;
    public int Damage => damage;
}

public enum projectileType
{
    Single, Triple
}
