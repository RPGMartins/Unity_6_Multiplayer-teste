using System;
using Unity.Netcode;
using UnityEngine;

public class Target : NetworkBehaviour
{
    private NetworkVariable<int> networkLife = new NetworkVariable<int>(0);
    private int localLife;
    private Action onDestryoAction;
    private bool conected = false;

    public override void OnNetworkSpawn()
    {
        networkLife.OnValueChanged += UpdateLocalLife;
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        networkLife.OnValueChanged -= UpdateLocalLife;
        base.OnNetworkDespawn();
    }

    private void Start()
    {
        conected = NetworkConnectionChecker.CheckConnection();
    }

    public void Initialize(int life, Action destroyAction)
    {
        UpdateLife(life);
        onDestryoAction = destroyAction;
    }

    public void Hit(int damage)
    {
        if (NetworkConnectionChecker.CheckConnection() == false)
        {
            TakeDamageLocal(damage);

            return;
        }

        if (NetworkConnectionChecker.CheckIfHasAuthority() == false)
        {
            TakeDamageRPC(damage);
        }
        else
        {
            TakeDamageNetwork(damage);
        }
    }

    [Rpc(SendTo.Server)]
    private void TakeDamageRPC(int damage)
    {
        TakeDamageNetwork(damage);
    }

    private void TakeDamageLocal(int damage)
    {
        TakeDamage(damage);
    }

    private void TakeDamageNetwork(int damage)
    {
        TakeDamage(damage);
    }

    private void TakeDamage(int damage)
    {
        if (UpdateLife(damage) == false)
        {
            //Efeitos de destruição, talvez esperar algo
            onDestryoAction.Invoke();
        }
        else
        {
            //Efeitos de hit
        }
    }

    private bool UpdateLife(int damage)
    {
        if (conected == true)
        {
            UpdateNetworkLife(networkLife.Value, networkLife.Value - damage);

            return networkLife.Value >= 1;
        }

        UpdateLocalLife(localLife, localLife - damage);

        return localLife >= 1;
    }

    private void UpdateLocalLife(int previousValue, int newValue)
    {
        localLife = newValue;
    }

    private void UpdateNetworkLife(int previousValue, int newValue)
    {
        networkLife.Value = newValue;
    }
}
