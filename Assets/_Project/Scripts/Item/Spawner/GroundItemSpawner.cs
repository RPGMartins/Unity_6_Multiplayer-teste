using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class GroundItemSpawner : MonoBehaviour
{
    [SerializeField] private bool drawDebug;
    [SerializeField] private GameObject groundItemPrefab;
    [SerializeField] private GameObject networkGroundItemPrefab;

    [SerializeField] private Vector2 rangeToSpawn;

    private List<GroundItem> spawnedGroundItems = new List<GroundItem>();

    public void Spawn(ItemData itemData)
    {
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsListening == false)
        {
            GroundItem spawnedGroundItem = SpawnGroundItem(itemData);
            spawnedGroundItems.Add(spawnedGroundItem);
        }
        else
        {
            GroundItem spawnedGroundItem = NetworkSpawnGroundItem(itemData);
            spawnedGroundItems.Add(spawnedGroundItem);
        }
    }

    private GroundItem SpawnGroundItem(ItemData itemData)
    {
        return SpawnPrefab(itemData, groundItemPrefab);
    }

    private GroundItem NetworkSpawnGroundItem(ItemData itemData)
    {
        GroundItem groundItem = SpawnPrefab(itemData, networkGroundItemPrefab);
        NetworkObject networkObject = groundItem.GetComponentInParent<NetworkObject>();
        networkObject.Spawn(true);

        return groundItem;
    }

    private GroundItem SpawnPrefab(ItemData itemData, GameObject itemPrefab)
    {
        Vector3 spawnOffset = new Vector2(Random.Range(-rangeToSpawn.x, rangeToSpawn.x), Random.Range(-rangeToSpawn.y, rangeToSpawn.y));
        GameObject groundItemInstance = Instantiate(itemPrefab, transform.position + spawnOffset, Quaternion.identity, transform);
        GroundItem groundItem = groundItemInstance.GetComponentInChildren<GroundItem>();
        groundItem.Initialize(itemData.ToBaseItem(), DespawnPrefab);

        return groundItem;
    }

    private void DespawnPrefab(GroundItem groundItem)
    {
        spawnedGroundItems.Remove(groundItem);
        Destroy(groundItem.ParentGameObject);
    }

    private void OnDrawGizmos()
    {
        if (drawDebug == false)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, rangeToSpawn);
    }
}
