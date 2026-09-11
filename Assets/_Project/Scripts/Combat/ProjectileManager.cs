using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectileManager : NetworkBehaviour
{
    public static ProjectileManager Instance { get; private set; }

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileHolder;

    [SerializeField] private List<ProjectileData> projectileDataList;

    private ObjectPool<Projectile> projectilePool;
    private ProjectilePrefabInstanceHandler projectilePrefabInstanceHandler;

    private List<Projectile> currentProjectiles = new List<Projectile>();

    public override void OnNetworkSpawn()
    {
        projectilePrefabInstanceHandler = new ProjectilePrefabInstanceHandler();
        NetworkManager.PrefabHandler.AddHandler(projectilePrefab, projectilePrefabInstanceHandler);
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.PrefabHandler.RemoveHandler(projectilePrefab);
    }

    private void Awake()
    {
        Instance = this;
        projectilePool = new ObjectPool<Projectile>(CreateLocalProjectile, GetLocalProjectile, ReleaseLocalProjectile, ActionOnDestroy);
    }

    private void Update()
    {
        if (NetworkConnectionChecker.CheckConnection() == true)
        {
            if (NetworkConnectionChecker.CheckIfHasAuthority() == false)
            {
                return;
            }
        }

        for (int i = currentProjectiles.Count - 1; i >= 0; i--)
        {
            Projectile projectile = currentProjectiles[i];

            if (projectile.IsFarFromOrigin(40) == false)
            {
                continue;
            }

            IvokeOnProjectileExpired(projectile);
        }
    }

    public override void OnDestroy()
    {
        projectilePool.Dispose();
    }

    public void Shoot(Vector3 originPosition, Vector3 targetPosition, Quaternion rotation, FixedString64Bytes weaponID)
    {
        if (NetworkConnectionChecker.CheckConnection() == false)
        {
            CreateShoot(originPosition, targetPosition, rotation, weaponID);
        }
        else
        {
            if (NetworkConnectionChecker.CheckIfHasAuthority() == false)
            {
                ShootRPC(originPosition, targetPosition, rotation, weaponID);
            }
            else
            {
                ShootServer(originPosition, targetPosition, rotation, weaponID);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void ShootRPC(Vector3 originPosition, Vector3 targetPosition, Quaternion rotation, FixedString64Bytes weaponID)
    {
        ShootServer(originPosition, targetPosition, rotation, weaponID);
    }

    private void ShootServer(Vector3 originPosition, Vector3 targetPosition, Quaternion rotation, FixedString64Bytes weaponID)
    {
        CreateShoot(originPosition, targetPosition, rotation, weaponID);
    }

    private void CreateShoot(Vector3 originPosition, Vector3 targetPosition, Quaternion rotation, FixedString64Bytes weaponID)
    {
        ProjectileData projectileData = GetProjectileData(weaponID.ToString());
        InitializeShoot(originPosition, targetPosition, projectileData);
    }

    private void InitializeShoot(Vector3 originPosition, Vector3 targetPosition, ProjectileData projectileData)
    {
        switch (projectileData.ProjectileType)
        {
            case projectileType.Single:
                InitializeProjectile(targetPosition - originPosition);

                break;

            case projectileType.Triple:

                float spreadAngle = 15f;
                Vector3 direction = (targetPosition - originPosition).normalized;
                Vector3 upperDirection = Quaternion.AngleAxis(spreadAngle, Vector3.forward) * direction;
                Vector3 lowerDirection = Quaternion.AngleAxis(-spreadAngle, Vector3.forward) * direction;

                for (int i = 0; i < 3; i++)
                {
                    Vector3 tempDirection = direction;

                    if (i == 1)
                    {
                        tempDirection = upperDirection;
                    }
                    else if (i == 2)
                    {
                        tempDirection = lowerDirection;
                    }

                    InitializeProjectile(tempDirection);
                }

                break;
        }

        void InitializeProjectile(Vector3 direction)
        {
            Projectile projectile = GetProjectileFromPool();

            if (NetworkConnectionChecker.CheckConnection() == true)
            {
                ProjectileSpawnData projectileSpawnData = new ProjectileSpawnData(originPosition, direction, projectileData.Velocity, projectileData.Damage);

                projectile.Initialize(originPosition, direction, projectileData.Velocity, projectileData.Damage);
                NetworkManager.PrefabHandler.SetInstantiationData(projectile.NetworkObject, projectileSpawnData);
                projectile.NetworkObject.Spawn();
            }
            else
            {
                projectile.Initialize(originPosition, direction, projectileData.Velocity, projectileData.Damage);
            }
        }
    }

    #region Pool

    public Projectile GetProjectileFromPool()
    {
        Projectile projectile = projectilePool.Get();
        currentProjectiles.Add(projectile);

        return projectile;
    }

    public void DespawnProjectile(Projectile projectile)
    {
        if (NetworkConnectionChecker.CheckConnection() == true)
        {
            if (NetworkConnectionChecker.CheckIfHasAuthority() == false)
            {
                return;
            }

            projectile.NetworkObject.Despawn(false);
            ReleaseProjectileLocal(projectile);

            return;
        }

        ReleaseProjectileLocal(projectile);
    }

    private Projectile CreateLocalProjectile()
    {
        GameObject projectileGameObject = Instantiate(projectilePrefab, projectileHolder);
        Projectile projectile = projectileGameObject.GetComponentInChildren<Projectile>();
        projectile.ProjectileTransformHolder.gameObject.SetActive(false);

        return projectile;
    }

    private void GetLocalProjectile(Projectile projectile)
    {
        projectile.ProjectileTransformHolder.gameObject.SetActive(true);
    }

    private void ReleaseLocalProjectile(Projectile projectile)
    {
        projectile.ResetVariables();
        projectile.ProjectileTransformHolder.gameObject.SetActive(false);
    }

    private void ActionOnDestroy(Projectile projectile)
    {
        Destroy(projectile.ProjectileTransformHolder);
    }

    #endregion

    public void ReleaseProjectileLocal(Projectile projectile)
    {
        currentProjectiles.Remove(projectile);
        projectilePool.Release(projectile);
    }

    public void IvokeOnProjectileExpired(Projectile projectile)
    {
        if (projectile.NetworkObject.IsSpawned)
        {
            DespawnProjectile(projectile);
        }
        else
        {
            ReleaseProjectileLocal(projectile);
        }
    }

    private ProjectileData GetProjectileData(string weaponID)
    {
        foreach (ProjectileData projectileData in projectileDataList)
        {
            if (projectileData.WeaponID == weaponID)
            {
                return projectileData;
            }
        }

        throw new Exception("ID de arma faltando");
    }
}
