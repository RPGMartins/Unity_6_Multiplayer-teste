using Unity.Netcode;
using UnityEngine;

public class NetworkProjectile : NetworkBehaviour
{
    [SerializeField] private LocalProjectile localProjectile;

    private NetworkVariable<Vector3> originalPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Vector3> direction = new NetworkVariable<Vector3>();
    private NetworkVariable<float> velocity = new NetworkVariable<float>();

    public void Initialize(Vector3 originalPosition, Vector3 direction, float velocity)
    {
        this.originalPosition.Value = originalPosition;
        this.direction.Value = direction;
        this.velocity.Value = velocity;
    }

    public override void OnNetworkSpawn()
    {
        localProjectile.Initialize(originalPosition.Value, direction.Value, velocity.Value);
    }
}
