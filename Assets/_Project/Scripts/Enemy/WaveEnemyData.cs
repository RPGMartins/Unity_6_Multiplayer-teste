using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveEnemyData", menuName = "Wave/WaveEnemyData")]
public class WaveEnemyData : ScriptableObject
{
    [SerializeField] private List<WaveEnemy> waveEnemies;

    public List<WaveEnemy> WaveEnemies => waveEnemies;
}

[Serializable]
public struct WaveEnemy
{
    [SerializeField] private float timeBetweenWaves;
    [SerializeField] private float timeBetweenSpawns;
    [SerializeField] private int enemyQuantity;

    public int EnemyQuantity => enemyQuantity;
    public float TimeBetweenSpawns => timeBetweenSpawns;
    public float TimeBetweenWaves => timeBetweenWaves;
}
