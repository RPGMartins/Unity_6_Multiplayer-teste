using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveEnemyManager : MonoBehaviour
{
    [SerializeField] private WaveEnemyData waveEnemyData;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int life;
    [SerializeField] private Vector2 offsetSpawn;

    public event Action onFinishedAllWaves;
    public event Action onFinishedWave;

    private List<EnemyInitializer> enemyInitializerList = new List<EnemyInitializer>();
    private int waveIndex;
    private int enemiesToSpawn;
    private float timeToSpawn;
    private float timeToWave;
    private bool finishedWaves;
    private bool hasConnection;
    public static WaveEnemyManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        hasConnection = NetworkConnectionChecker.CheckConnection();

        EnemyInitializer[] enemyInitializers = GetComponentsInChildren<EnemyInitializer>();

        foreach (EnemyInitializer enemyInitializer in enemyInitializers)
        {
            enemyInitializerList.Add(enemyInitializer);
        }
    }

    private void Start()
    {
        ResetVariables();
        GetWaveInfos();
    }

    private void Update()
    {
        if (finishedWaves == true)
        {
            return;
        }

        timeToWave -= Time.deltaTime;

        if (timeToWave > 0)
        {
            return;
        }

        timeToSpawn -= Time.deltaTime;

        if (timeToSpawn > 0)
        {
            return;
        }

        SpawnEnemy();
        enemiesToSpawn--;

        if (enemiesToSpawn <= 0)
        {
            waveIndex++;
            onFinishedWave?.Invoke();

            if (waveEnemyData.WaveEnemies.Count >= waveIndex)
            {
                finishedWaves = true;
                onFinishedAllWaves?.Invoke();

                return;
            }

            GetWaveInfos();
        }

        timeToSpawn = waveEnemyData.WaveEnemies[waveIndex].TimeBetweenSpawns;
    }

    private void GetWaveInfos()
    {
        enemiesToSpawn = waveEnemyData.WaveEnemies[waveIndex].EnemyQuantity;
        timeToSpawn = waveEnemyData.WaveEnemies[waveIndex].TimeBetweenSpawns;
        timeToWave = waveEnemyData.WaveEnemies[waveIndex].TimeBetweenWaves;
    }

    private void SpawnEnemy()
    {
        EnemyInitializer randomEnemyInitializer = enemyInitializerList[Random.Range(0, enemyInitializerList.Count)];
        Vector2 spawnPosition = randomEnemyInitializer.transform.position + new Vector3(Random.Range(-offsetSpawn.x, offsetSpawn.x), Random.Range(-offsetSpawn.y, offsetSpawn.y));

        if (hasConnection == true)
        {
            if (NetworkManager.Singleton.IsServer == false)
            {
                return;
            }

            randomEnemyInitializer.SpawnEnemyNetwork(enemyPrefab, spawnPosition, life);
        }
        else
        {
            randomEnemyInitializer.SpawnEnemyLocal(enemyPrefab, spawnPosition, life);
        }
    }

    private void ResetVariables()
    {
        waveIndex = 0;
        enemiesToSpawn = 0;
        timeToSpawn = 0f;
        timeToWave = 0f;
        finishedWaves = false;
    }
}
