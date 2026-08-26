using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileHolder;

    private ObjectPool<LocalProjectile> projectilePool;

    private List<LocalProjectile> currentProjectiles = new List<LocalProjectile>();

    public event Action<LocalProjectile> OnProjectileExpired;

    private void Awake()
    {
        Instance = this;
        projectilePool = new ObjectPool<LocalProjectile>(CreateLocalProjectile, GetLocalProjectile, ReleaseLocalProjectile, ActionOnDestroy);
    }

    private void Update()
    {
        for (int i = currentProjectiles.Count - 1; i >= 0; i--)
        {
            LocalProjectile localProjectile = currentProjectiles[i];

            if (localProjectile.IsFarFromOrigin(40) == false)
            {
                continue;
            }

            OnProjectileExpired?.Invoke(localProjectile);
        }
    }

    private void OnDestroy()
    {
        projectilePool.Dispose();
    }

    public LocalProjectile Shoot(Vector3 originPosition, Vector3 targetPosition, Quaternion rotation, FixedString64Bytes weaponID, bool initialize = true)
    {
        LocalProjectile localProjectile = projectilePool.Get();

        if (initialize == true)
        {
            localProjectile.Initialize(originPosition, targetPosition - originPosition, 5);
        }

        currentProjectiles.Add(localProjectile);

        return localProjectile;
    }

    public void ReturnProjectile(LocalProjectile projectile)
    {
        currentProjectiles.Remove(projectile);
        projectilePool.Release(projectile);
    }

    private LocalProjectile CreateLocalProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, projectileHolder);
        LocalProjectile localProjectile = projectile.GetComponentInChildren<LocalProjectile>();
        localProjectile.ProjectileTransformHolder.gameObject.SetActive(false);

        return localProjectile;
    }

    private void GetLocalProjectile(LocalProjectile localProjectile)
    {
        localProjectile.ProjectileTransformHolder.gameObject.SetActive(true);
    }

    private void ReleaseLocalProjectile(LocalProjectile localProjectile)
    {
        localProjectile.ResetVariables();
        localProjectile.ProjectileTransformHolder.gameObject.SetActive(false);
    }

    private void ActionOnDestroy(LocalProjectile localProjectile)
    {
        Destroy(localProjectile.ProjectileTransformHolder);
    }
}
