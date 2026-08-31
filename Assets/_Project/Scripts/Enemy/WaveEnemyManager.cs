using System.Collections.Generic;
using UnityEngine;

public class WaveEnemyManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    private List<EnemyInitializer> enemyInitializerList = new List<EnemyInitializer>();
    private bool hasConnection;

    private void Awake()
    {
        hasConnection = NetworkConnectionChecker.CheckConnection();

        EnemyInitializer[] enemyInitializers = GetComponentsInChildren<EnemyInitializer>();

        foreach (EnemyInitializer enemyInitializer in enemyInitializers)
        {
            enemyInitializerList.Add(enemyInitializer);
        }
    }
}
