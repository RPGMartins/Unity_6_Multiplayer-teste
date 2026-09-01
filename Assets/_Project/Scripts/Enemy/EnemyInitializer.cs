using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyInitializer : MonoBehaviour
{
    public void SpawnEnemyNetwork(GameObject enemyPrefab, Vector3 position, int life)
    {
        GameObject enemyGameObjectInstance = SpawnBase(enemyPrefab, position, out Action destroyAction);

        NetworkEnemyController networkEnemyController = enemyGameObjectInstance.GetComponent<NetworkEnemyController>();
        networkEnemyController.Initialize(life, destroyAction);
        NetworkObject networkObject = networkEnemyController.GetComponent<NetworkObject>();
        networkObject.Spawn();
    }

    public void SpawnEnemyLocal(GameObject enemyPrefab, Vector3 position, int life)
    {
        GameObject enemyGameObjectInstance = SpawnBase(enemyPrefab, position, out Action destroyAction);
        enemyGameObjectInstance.GetComponentInChildren<LocalEnemyController>().Initialize(life, destroyAction, false);
    }

    private GameObject SpawnBase(GameObject enemyPrefab, Vector3 position, out Action destroyAction)
    {
        GameObject enemyGameObjectInstance = Instantiate(enemyPrefab, position, Quaternion.identity);
        destroyAction = () => Destroy(enemyGameObjectInstance);

        return enemyGameObjectInstance;
    }
}
