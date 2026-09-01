using Unity.Netcode;
using UnityEngine;

public class NetworkProjectile : NetworkBehaviour
{
    [SerializeField] private LocalProjectile localProjectile;

    private NetworkVariable<Vector3> originalPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Vector3> direction = new NetworkVariable<Vector3>();
    private NetworkVariable<float> velocity = new NetworkVariable<float>();
    private NetworkVariable<int> damage = new NetworkVariable<int>();

    public void InitializeServer(Vector3 originalPosition, Vector3 newDirection, float velocity, int damage)
    {
        if (IsServer == false || IsSpawned == false)
        {
            return;
        }

        this.originalPosition.Value = originalPosition;
        this.direction.Value = newDirection;
        this.velocity.Value = velocity;
        this.damage.Value = damage;
    }

    public override void OnNetworkSpawn()
    {
        direction.OnValueChanged += OnDirectionChanged;
        originalPosition.OnValueChanged += OnDirectionChanged;
        velocity.OnValueChanged += OnSpeedChanged;

        TryInitialize();
    }

    public override void OnNetworkDespawn()
    {
        direction.OnValueChanged -= OnDirectionChanged;
        velocity.OnValueChanged -= OnSpeedChanged;
        originalPosition.OnValueChanged -= OnDirectionChanged;
    }

    private void OnDirectionChanged(Vector3 oldValue, Vector3 newValue)
    {
        TryInitialize();
    }

    private void OnSpeedChanged(float oldValue, float newValue)
    {
        TryInitialize();
    }

    private void TryInitialize()
    {
        localProjectile.Initialize(originalPosition.Value, direction.Value, velocity.Value, damage.Value);
    }
}
