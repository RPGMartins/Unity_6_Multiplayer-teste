using UnityEngine;

public class GroundItemSpawner : MonoBehaviour
{
    [SerializeField] private GroundItem itemPrefab;
    [SerializeField] private ItemData itemData;

    [SerializeField] private Vector2 rangeToSpawn;

    public void Start()
    {
        GroundItem groundItem = Instantiate(itemPrefab);
        groundItem.Initialize(itemData.ToBaseItem());
        Vector3 spawnOffset = new Vector2(Random.Range(-rangeToSpawn.x, rangeToSpawn.x), Random.Range(-rangeToSpawn.y, rangeToSpawn.y));
        groundItem.transform.position = transform.position + spawnOffset;
    }
}
