using System;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    public void SpawnEnemy(Vector3 spawnPosition, int life)
    {
        if (NetworkConnectionChecker.CheckConnection() == false)
        {
            SpawnEnemyLocal(enemyPrefab, spawnPosition, life);

            return;
        }

        if (NetworkConnectionChecker.CheckIfHasAuthority() == true)
        {
            SpawnEnemyNetwork(enemyPrefab, spawnPosition, life);
        }
    }

    private void SpawnEnemyNetwork(GameObject enemyPrefab, Vector3 position, int life)
    {
        EnemyController enemyController = SpawnEnemyLocal(enemyPrefab, position, life);
        enemyController.NetworkObject.Spawn();
    }

    private EnemyController SpawnEnemyLocal(GameObject enemyPrefab, Vector3 position, int life)
    {
        EnemyController enemyController = SpawnBase(enemyPrefab, position, out Action destroyAction);
        enemyController.Initialize(life, destroyAction);

        return enemyController;
    }

    private EnemyController SpawnBase(GameObject enemyPrefab, Vector3 position, out Action destroyAction)
    {
        GameObject enemyGameObjectInstance = Instantiate(enemyPrefab, position, Quaternion.identity);
        destroyAction = () => Destroy(enemyGameObjectInstance);

        return enemyGameObjectInstance.GetComponent<EnemyController>();
    }
}
