using System;
using UnityEngine;

public class ProjectileCollision : MonoBehaviour
{
    private Action onHitTarget;
    private int damage;

    public void Initialize(Action onHitTarget, int damage)
    {
        this.onHitTarget = onHitTarget;
        this.damage = damage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("target") == false)
        {
            return;
        }

        Target target = other.transform.parent.GetComponentInChildren<Target>();

        if (target != null)
        {
            target.Hit(damage);
            onHitTarget?.Invoke();
        }
    }
}
