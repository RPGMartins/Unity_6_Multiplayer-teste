using Unity.Netcode;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Transform projectileTransformHolder;
    [SerializeField] private ProjectileCollision projectileCollision;
    [SerializeField] private NetworkObject networkObject;

    private Vector3 direction;
    private float velocity;
    private int damage;
    private bool initialized = false;
    private Vector3 originPosition;

    public Transform ProjectileTransformHolder => projectileTransformHolder;
    public NetworkObject NetworkObject => networkObject;
    public float Velocity => velocity;
    public int Damage => damage;

    private void Update()
    {
        if (initialized == false)
        {
            return;
        }

        projectileTransformHolder.position += direction * (velocity * Time.deltaTime);
    }

    public void Initialize(Vector3 originalPosition, Vector3 direction, float velocity, int damage)
    {
        projectileTransformHolder.transform.position = originalPosition;
        this.direction = direction.normalized;
        this.velocity = velocity;
        this.initialized = true;
        this.originPosition = originalPosition;
        this.damage = damage;

        projectileCollision.Initialize(InvokeProjectExpired, damage);

        void InvokeProjectExpired()
        {
            ProjectileManager.Instance.IvokeOnProjectileExpired(this);
        }
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
