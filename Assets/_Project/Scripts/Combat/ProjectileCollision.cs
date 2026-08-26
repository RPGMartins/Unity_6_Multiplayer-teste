using System;
using UnityEngine;

public class ProjectileCollision : MonoBehaviour
{
    private Action onHitTarget;

    public void Initilaize(Action onHitTarget)
    {
        this.onHitTarget = onHitTarget;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        NetworkTarget networkTarget = other.transform.parent.GetComponent<NetworkTarget>();

        if (networkTarget != null && networkTarget.IsSpawned == true)
        {
            networkTarget.TakeDamage(1);

            return;
        }

        LocalTarget localTarget = other.transform.parent.GetComponentInChildren<LocalTarget>();

        if (localTarget == null)
        {
            return;
        }

        localTarget.Hit(1);

        if (onHitTarget != null)
        {
            onHitTarget.Invoke();
        }
    }
}
