using System;
using Unity.Netcode;
using UnityEngine;

public class TargetInitializer : MonoBehaviour
{
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private int defaultLifeValue = 3;
    [SerializeField] private bool spawnTarget;

    private bool hasConnection;

    private void Awake()
    {
        hasConnection = NetworkConnectionChecker.CheckConnection();
    }

    private void Update()
    {
        if (spawnTarget == true)
        {
            SpawnTarget();
            spawnTarget = false;
        }
    }

    private void SpawnTarget()
    {
        GameObject targetInstance = Instantiate(targetPrefab);

        if (hasConnection == true)
        {
            NetworkTarget networkTarget = targetInstance.GetComponent<NetworkTarget>();
            networkTarget.Initialize(defaultLifeValue);
            networkTarget.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            LocalTarget localTarget = targetInstance.GetComponentInChildren<LocalTarget>();
            localTarget.Initialize(defaultLifeValue);
        }
    }
}
