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
            return;
        }

        ShootServer(originPosition, targetPosition, rotation, weaponID);
    }

    private void ShootServer(Vector3 originPosition, Vector3 targetPosition, Quaternion rotation, FixedString64Bytes weaponID)
    {
        LocalProjectile projectile = ProjectileManager.Instance.Shoot(originPosition, targetPosition, rotation, weaponID, false);
        NetworkObject networkObject = projectile.GetComponentInParent<NetworkObject>();
        networkObject.GetComponent<NetworkProjectile>().Initialize(originPosition, targetPosition - originPosition, 5);
        networkObject.Spawn();
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
}
