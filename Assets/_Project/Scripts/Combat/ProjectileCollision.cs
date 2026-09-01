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

        NetworkTarget networkTarget = other.transform.parent.GetComponent<NetworkTarget>();

        if (networkTarget != null && networkTarget.IsSpawned == true)
        {
            networkTarget.TakeDamage(damage);
            onHitTarget?.Invoke();

            return;
        }

        LocalTarget localTarget = other.transform.parent.GetComponentInChildren<LocalTarget>();

        if (localTarget != null)
        {
            localTarget.Hit(damage);
            onHitTarget?.Invoke();
        }
    }
}
