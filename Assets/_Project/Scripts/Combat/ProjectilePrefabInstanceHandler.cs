using Unity.Netcode;
using UnityEngine;

public class ProjectilePrefabInstanceHandler : INetworkPrefabInstanceHandler
{
    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation) // é chamado somente no client side quando o spawn chama
    {
        LocalProjectile projectile = ProjectileManager.Instance.GetProjectileFromPool();
        NetworkObject networkObject = projectile.GetComponentInParent<NetworkObject>();
        networkObject.transform.SetPositionAndRotation(position, rotation);

        return networkObject;
    }

    public void Destroy(NetworkObject networkObject)
    {
        LocalProjectile projectile = networkObject.GetComponentInChildren<LocalProjectile>(true);
        ProjectileManager.Instance.ReturnProjectile(projectile);
    }
}
