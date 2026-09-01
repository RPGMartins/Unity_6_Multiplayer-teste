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

    [SerializeField] private List<ProjectileData> projectileDataList;

    private ObjectPool<LocalProjectile> projectilePool;

    private List<LocalProjectile> currentProjectiles = new List<LocalProjectile>();

    public event Action<LocalProjectile> OnProjectileExpired;

    public GameObject ProjectilePrefab => projectilePrefab;

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

            IvokeOnProjectileExpired(localProjectile);
        }
    }

    private void OnDestroy()
    {
        projectilePool.Dispose();
    }

    public List<LocalProjectile> Shoot(Vector3 originPosition, Vector3 targetPosition, Quaternion rotation, FixedString64Bytes weaponID)
    {
        ProjectileData projectileData = GetProjectileData(weaponID.ToString());
        List<LocalProjectile> localProjectiles = Shoot(originPosition, targetPosition, projectileData);
        currentProjectiles.AddRange(localProjectiles);

        return localProjectiles;
    }

    public List<LocalProjectile> Shoot(Vector3 originPosition, Vector3 targetPosition, ProjectileData projectileData)
    {
        switch (projectileData.ProjectileType)
        {
            case projectilType.single:
                LocalProjectile localProjectile = GetProjectileFromPool();
                localProjectile.Initialize(originPosition, targetPosition - originPosition, projectileData.Velocity, projectileData.Damage);

                return new List<LocalProjectile>() { localProjectile };

            case projectilType.triple:

                List<LocalProjectile> projectileList = new List<LocalProjectile>();
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

                    LocalProjectile tempProjectile = GetProjectileFromPool();
                    tempProjectile.Initialize(originPosition, tempDirection, projectileData.Velocity, projectileData.Damage);
                    projectileList.Add(tempProjectile);
                }

                return projectileList;
        }

        return null;
    }

    public LocalProjectile GetProjectileFromPool()
    {
        return projectilePool.Get();
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

    public void IvokeOnProjectileExpired(LocalProjectile localProjectile)
    {
        OnProjectileExpired?.Invoke(localProjectile);
    }

    public ProjectileData GetProjectileData(string weaponID)
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

[Serializable]
public struct ProjectileData
{
    [SerializeField] private string weaponID;
    [SerializeField] private projectilType projectilType;
    [SerializeField] private float velocity;
    [SerializeField] private int damage;

    public string WeaponID => weaponID;
    public projectilType ProjectileType => projectilType;
    public float Velocity => velocity;
    public int Damage => damage;
}

public enum projectilType
{
    single, triple
}
