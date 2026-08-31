using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkTarget : NetworkBehaviour
{
    [SerializeField] private LocalTarget localTarget;

    private NetworkVariable<int> life = new NetworkVariable<int>();

    private int tempLife = 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer == false)
        {
            return;
        }

        life.OnValueChanged += OnLifeValueChanged;
        life.Value = tempLife;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer == false)
        {
            return;
        }

        life.OnValueChanged -= OnLifeValueChanged;
    }

    public void Initialize(int life, Action actionOnToDestroy)
    {
        tempLife = life;
        localTarget.Initialize(life, actionOnToDestroy);
    }

    public void TakeDamage(int damage)
    {
        if (IsServer == false)
        {
            return;
        }

        life.Value -= damage;
    }

    private void OnLifeValueChanged(int previousValue, int newValue)
    {
        if (IsServer == false)
        {
            return;
        }

        int damage = previousValue - newValue;

        if (damage > 0)
        {
            if (newValue > 0)
            {
                localTarget.TakeDamage(previousValue - newValue);
            }
            else
            {
                localTarget.DestroyEffect();
            }
        }
    }
}
