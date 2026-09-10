using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GroundItemSpawnerController : MonoBehaviour
{
    public static GroundItemSpawnerController instance;
    [SerializeField] private List<ItemData> availableItems = new List<ItemData>();
    [SerializeField] private GroundItemSpawner groundItemSpawner;
    [SerializeField] private float intervalToSpawnItem;
    private Dictionary<ulong, GroundItem> spawnedGroundItemsDictionary = new Dictionary<ulong, GroundItem>();

    private ulong itemIdentificationID = 0;
    private float currentTime;
    private bool network;

    public Dictionary<ulong, GroundItem> SpawnedGroundItemsDictionary => spawnedGroundItemsDictionary;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        network = false;

        if (NetworkConnectionChecker.CheckConnection() == true)
        {
            if (NetworkConnectionChecker.CheckIfHasAuthority() == false)
            {
                enabled = false;

                return;
            }

            network = true;
        }

        ResetTimer();
    }

    private void Update()
    {
        currentTime -= Time.deltaTime;

        if (currentTime > 0)
        {
            return;
        }

        ResetTimer();
        GroundItem groundItem = groundItemSpawner.Spawn(availableItems[Random.Range(0, availableItems.Count)], itemIdentificationID, network);
        spawnedGroundItemsDictionary.Add(itemIdentificationID, groundItem);

        itemIdentificationID++;
    }

    private void ResetTimer()
    {
        currentTime = intervalToSpawnItem;
    }

    public void DestroyItem(ulong itemIdentificationID, bool network)
    {
        spawnedGroundItemsDictionary.Remove(itemIdentificationID, out GroundItem groundItem);
        groundItemSpawner.Despawn(groundItem, network);
    }
}
