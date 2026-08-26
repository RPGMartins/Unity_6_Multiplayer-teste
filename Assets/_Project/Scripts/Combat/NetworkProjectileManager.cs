using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkProjectileManager : NetworkBehaviour
{
    public static NetworkProjectileManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void Shoot(Vector3 originPosition, Vector3 targetPosition, Quaternion rotation, FixedString64Bytes weaponID)
    {
        if (IsServer == false)
        {
            ShootRPC(originPosition, targetPosition, rotation, weaponID);

            return;
        }

        ShootServer(originPosition, targetPosition, rotation, weaponID);
    }

    private void ShootServer(Vector3 originPosition, Vector3 targetPosition, Quaternion rotation, FixedString64Bytes weaponID)
    {
        LocalProjectile projectile = ProjectileManager.Instance.Shoot(originPosition, targetPosition, rotation, weaponID, false);
        NetworkObject networkObject = projectile.GetComponentInParent<NetworkObject>();
        networkObject.Spawn();
        networkObject.GetComponent<NetworkProjectile>().InitializeServer(originPosition, targetPosition - originPosition, 5);
    }

    public void ReturnProjectile(LocalProjectile projectile)
    {
        if (IsServer == false)
        {
            return;
        }

        NetworkObject networkObject = projectile.GetComponentInParent<NetworkObject>();

        if (networkObject.IsSpawned)
        {
            networkObject.Despawn(false);
        }

        ProjectileManager.Instance.ReturnProjectile(projectile);
    }

    [Rpc(SendTo.Server)]
    private void ShootRPC(Vector3 originPosition, Vector3 targetPosition, Quaternion rotation, FixedString64Bytes weaponID)
    {
        ShootServer(originPosition, targetPosition, rotation, weaponID);
    }
}
