using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveEnemyManager : NetworkBehaviour
{
    [SerializeField] private EnemyInitializer enemyInitializer;
    [SerializeField] private WaveEnemyData waveEnemyData;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int life;
    [SerializeField] private Vector2 offsetSpawn;

    private ProjectilePrefabInstanceHandler projectilePrefabInstanceHandler;

    public event Action onFinishedAllWaves;
    public event Action onFinishedWave;

    private int waveIndex;
    private int enemiesToSpawn;
    private float timeToSpawn;
    private float timeToWave;
    private bool finishedWaves;
    public static WaveEnemyManager Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        projectilePrefabInstanceHandler = new ProjectilePrefabInstanceHandler();
        NetworkManager.PrefabHandler.AddHandler(enemyPrefab, projectilePrefabInstanceHandler);
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.PrefabHandler.RemoveHandler(enemyPrefab);
    }
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ResetVariables();
        GetWaveInfos();

        if (NetworkConnectionChecker.CheckConnection() == true && NetworkConnectionChecker.CheckIfHasAuthority() == false)
        {
            enabled = false;
        }
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
        Vector2 spawnPosition = enemyInitializer.transform.position + new Vector3(Random.Range(-offsetSpawn.x, offsetSpawn.x), Random.Range(-offsetSpawn.y, offsetSpawn.y));
        enemyInitializer.SpawnEnemy(enemyPrefab, spawnPosition, life);
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
