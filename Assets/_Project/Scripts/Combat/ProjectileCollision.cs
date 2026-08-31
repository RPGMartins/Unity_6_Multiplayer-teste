using System;
using UnityEngine;

public class ProjectileCollision : MonoBehaviour
{
    private Action onHitTarget;

    public void Initialize(Action onHitTarget)
    {
        this.onHitTarget = onHitTarget;
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
            networkTarget.TakeDamage(1);
            onHitTarget?.Invoke();

            return;
        }

        LocalTarget localTarget = other.transform.parent.GetComponentInChildren<LocalTarget>();

        if (localTarget != null)
        {
            localTarget.Hit(1);
            onHitTarget?.Invoke();
        }
    }
}
