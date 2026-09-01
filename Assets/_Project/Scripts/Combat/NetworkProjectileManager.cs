using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkProjectileManager : NetworkBehaviour
{
    private ProjectilePrefabInstanceHandler projectilePrefabInstanceHandler;
    public static NetworkProjectileManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        projectilePrefabInstanceHandler = new ProjectilePrefabInstanceHandler();
        NetworkManager.PrefabHandler.AddHandler(ProjectileManager.Instance.ProjectilePrefab, projectilePrefabInstanceHandler);
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.PrefabHandler.RemoveHandler(ProjectileManager.Instance.ProjectilePrefab);
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
        ProjectileData projectileData = ProjectileManager.Instance.GetProjectileData(weaponID.ToString());
        List<LocalProjectile> projectileList = ProjectileManager.Instance.Shoot(originPosition, targetPosition, rotation, weaponID);

        foreach (LocalProjectile projectile in projectileList)
        {
            NetworkObject networkObject = projectile.GetComponentInParent<NetworkObject>();
            networkObject.Spawn();
            networkObject.GetComponent<NetworkProjectile>().InitializeServer(originPosition, targetPosition - originPosition, projectileData.Velocity, projectileData.Damage);
        }
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
