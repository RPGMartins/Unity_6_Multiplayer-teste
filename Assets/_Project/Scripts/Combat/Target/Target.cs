using System;
using Unity.Netcode;
using UnityEngine;

public class Target : NetworkBehaviour
{
    [SerializeField] private int life;
    private Action OndestryoAction;

    public void Initialize(int life, Action destroyAction)
    {
        this.life = life;
        OndestryoAction = destroyAction;
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
            //RPC
        }
        else
        {
            TakeDamageNetwork(damage);
        }
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
        life -= damage;

        if (life <= 0)
        {
            //Efeitos de destruição, talvez esperar algo
            OndestryoAction.Invoke();
        }
        else
        {
            //Efeitos de hit
        }
    }
}
