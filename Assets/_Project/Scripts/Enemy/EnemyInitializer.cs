using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyInitializer : MonoBehaviour
{
    public void SpawnEnemy(GameObject enemyPrefab)
    {
    }

    private void SpawnEnemyNetwork(GameObject enemyPrefab, int life)
    {
        GameObject enemyGameObjectInstance = Instantiate(enemyPrefab);
        Action destroyAction = () => Destroy(enemyGameObjectInstance);
        NetworkEnemyController networkEnemyController = enemyGameObjectInstance.GetComponent<NetworkEnemyController>();
        NetworkObject networkObject = networkEnemyController.GetComponent<NetworkObject>();
        networkEnemyController.Initialize(3, destroyAction);
        networkObject.Spawn();
    }

    private void SpawnEnemyLocal(GameObject enemyPrefab, int life)
    {
        GameObject enemyGameObjectInstance = Instantiate(enemyPrefab);
        Action destroyAction = () => Destroy(enemyGameObjectInstance);
        enemyGameObjectInstance.GetComponentInChildren<LocalEnemyController>().Initialize(3, destroyAction, false);
    }
}
