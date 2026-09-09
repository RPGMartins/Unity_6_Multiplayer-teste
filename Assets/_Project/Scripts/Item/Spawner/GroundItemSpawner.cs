using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class GroundItemSpawner : MonoBehaviour
{
    [SerializeField] private GroundItem groundItemPrefab;
    [SerializeField] private Vector2 rangeToSpawn;
    [SerializeField] private bool drawDebug;

    private Dictionary<ulong, GroundItem> spawnedGroundItemsDictionary = new Dictionary<ulong, GroundItem>();

    public void Spawn(ItemData itemData, ulong itemIndentificationID)
    {
        SpawnGroundItem(itemData, NetworkConnectionChecker.CheckIfHasAuthority(), itemIndentificationID);
    }

    private void SpawnGroundItem(ItemData itemData, bool network, ulong itemIdentificationID)
    {
        GroundItem groundItem = SpawnPrefab();

        if (network == true)
        {
            NetworkObject networkObject = groundItem.GetComponentInParent<NetworkObject>();
            networkObject.Spawn(true);
        }

        groundItem.Initialize(itemData.ItemID, network, itemIdentificationID);
        spawnedGroundItemsDictionary.Add(itemIdentificationID, groundItem);
    }

    private GroundItem SpawnPrefab()
    {
        Vector3 spawnOffset = new Vector2(Random.Range(-rangeToSpawn.x, rangeToSpawn.x), Random.Range(-rangeToSpawn.y, rangeToSpawn.y));
        GroundItem groundItem = Instantiate(groundItemPrefab, transform.position + spawnOffset, Quaternion.identity, null);

        return groundItem;
    }

    public void Despawn(ulong identificationID, bool network)
    {
        spawnedGroundItemsDictionary.Remove(identificationID, out GroundItem groundItem);

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
