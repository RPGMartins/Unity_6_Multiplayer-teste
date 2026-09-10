using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class GroundItemSpawner : MonoBehaviour
{
    [SerializeField] private GroundItem groundItemPrefab;
    [SerializeField] private Vector2 rangeToSpawn;
    [SerializeField] private bool drawDebug;

    public GroundItem Spawn(ItemData itemData, ulong itemIdentificationID, bool network)
    {
        return SpawnGroundItem(itemData, network, itemIdentificationID);
    }

    private GroundItem SpawnGroundItem(ItemData itemData, bool network, ulong itemIdentificationID)
    {
        GroundItem groundItem = SpawnPrefab();

        if (network == true)
        {
            NetworkObject networkObject = groundItem.GetComponentInParent<NetworkObject>();
            networkObject.Spawn(true);
        }

        groundItem.Initialize(itemData.ItemID, network, itemIdentificationID);

        return groundItem;
    }

    private GroundItem SpawnPrefab()
    {
        Vector3 spawnOffset = new Vector2(Random.Range(-rangeToSpawn.x, rangeToSpawn.x), Random.Range(-rangeToSpawn.y, rangeToSpawn.y));
        GroundItem groundItem = Instantiate(groundItemPrefab, transform.position + spawnOffset, Quaternion.identity, null);

        return groundItem;
    }

    public void Despawn(GroundItem groundItem, bool network)
    {
        if (network == true)
        {
            NetworkObject networkObject = groundItem.GetComponentInParent<NetworkObject>();
            networkObject.Despawn();

            return;
        }

        Destroy(groundItem.gameObject);
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
