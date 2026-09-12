using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyInitializer : MonoBehaviour
{
    
    public void SpawnEnemy(GameObject enemyPrefab, Vector3 spawnPosition, int life)
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
        LocalEnemyController localEnemyController = SpawnBase(enemyPrefab, position, out Action destroyAction);

        NetworkEnemyController networkEnemyController = localEnemyController.GetComponent<NetworkEnemyController>();
        networkEnemyController.Initialize(life, destroyAction);
        NetworkObject networkObject = networkEnemyController.GetComponent<NetworkObject>();
        networkObject.Spawn();
    }

    private void SpawnEnemyLocal(GameObject enemyPrefab, Vector3 position, int life)
    {
        LocalEnemyController localEnemyController = SpawnBase(enemyPrefab, position, out Action destroyAction);
        localEnemyController.Initialize(life, destroyAction, false);
    }

    private LocalEnemyController SpawnBase(GameObject enemyPrefab, Vector3 position, out Action destroyAction)
    {
        GameObject enemyGameObjectInstance = Instantiate(enemyPrefab, position, Quaternion.identity);
        destroyAction = () => Destroy(enemyGameObjectInstance);

        return enemyGameObjectInstance.GetComponent<LocalEnemyController>();
    }
}
