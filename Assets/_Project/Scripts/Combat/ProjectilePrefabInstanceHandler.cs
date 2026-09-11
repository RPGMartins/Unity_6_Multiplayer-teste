using Unity.Netcode;
using UnityEngine;

public class ProjectilePrefabInstanceHandler : NetworkPrefabInstanceHandlerWithData<ProjectileSpawnData>
{
    public override NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation, ProjectileSpawnData projectileSpawnData) // é chamado somente no client side quando o spawn chama
    {
        Projectile projectile = ProjectileManager.Instance.GetProjectileFromPool();
        projectile.Initialize(projectileSpawnData.Origin, projectileSpawnData.Direction, projectileSpawnData.Velocity, projectileSpawnData.Damage);

        return projectile.NetworkObject;
    }

    public override void Destroy(NetworkObject networkObject)
    {
        Projectile projectile = networkObject.GetComponentInChildren<Projectile>(true);
        ProjectileManager.Instance.ReleaseProjectileLocal(projectile);
    }
}
