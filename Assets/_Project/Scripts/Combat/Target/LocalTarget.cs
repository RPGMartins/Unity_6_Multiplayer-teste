using System;
using UnityEngine;

public class LocalTarget : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private int life;

    private Action actionToDestroy;

    public void Initialize(int life, Action actionToDestroy)
    {
        this.life = life;
        this.actionToDestroy = actionToDestroy;
    }

    public void Hit(int damage)
    {
        TakeDamage(damage);

        if (life <= 0)
        {
            DestroyEffect();
        }
    }

    public void TakeDamage(int damage)
    {
        life -= damage;

        //Efeitos de hit
    }

    public void DestroyEffect()
    {
        //Efeitos de destruição, talvez esperar algo
        actionToDestroy.Invoke();
    }
}
