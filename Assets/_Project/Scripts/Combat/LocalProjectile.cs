using System;
using UnityEngine;

public class LocalProjectile : MonoBehaviour
{
    [SerializeField] private Transform projectileTransformHolder;
    [SerializeField] private ProjectileCollision projectileCollision;
    private Vector3 direction;
    private float velocity;

    private bool initialized = false;
    private Vector3 originPosition;

    public Transform ProjectileTransformHolder => projectileTransformHolder;

    public void InitializeServer(Vector3 originalPositionValue, Vector3 directionValue, float velocityValue)
    {
        Initialize(originalPositionValue, directionValue, velocityValue, null);
    }

    public void Initialize(Vector3 originalPosition, Vector3 direction, float velocity, Action<LocalProjectile> onProjectileExpired)
    {
        projectileTransformHolder.transform.position = originalPosition;
        this.direction = direction.normalized;
        this.velocity = velocity;
        this.initialized = true;
        this.originPosition = originalPosition;

        if (onProjectileExpired != null)
        {
            projectileCollision.Initilaize(() => onProjectileExpired(this));
        }
    }

    private void Update()
    {
        if (initialized == false)
        {
            return;
        }

        projectileTransformHolder.position += direction * (velocity * Time.deltaTime);
    }

    public bool IsFarFromOrigin(float distance)
    {
        return Vector3.Distance(originPosition, projectileTransformHolder.position) >= distance;
    }

    public void ResetVariables()
    {
        direction = Vector3.zero;
        velocity = 0;
        initialized = false;
        originPosition = Vector3.zero;
        projectileTransformHolder.position = Vector3.zero;
    }
}
