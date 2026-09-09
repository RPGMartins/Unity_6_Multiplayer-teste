using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GroundItemSpawnerController : MonoBehaviour
{
    public static GroundItemSpawnerController instance;
    [SerializeField] private List<ItemData> availableItems = new List<ItemData>();
    [SerializeField] private GroundItemSpawner groundItemSpawner;
    [SerializeField] private float intervalToSpawnItem;

    private ulong itemIdentificationID = 0;

    private float currentTime;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
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
        groundItemSpawner.Spawn(availableItems[Random.Range(0, availableItems.Count)], itemIdentificationID);
        itemIdentificationID++;
    }

    private void ResetTimer()
    {
        currentTime = intervalToSpawnItem;
    }

    public void DestroyItem(ulong itemIdentificationID, bool network)
    {
        groundItemSpawner.Despawn(itemIdentificationID, network);
    }
}
