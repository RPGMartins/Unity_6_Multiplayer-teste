using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GroundItemSpawnerController : MonoBehaviour
{
    [SerializeField] private List<ItemData> availableItems = new List<ItemData>();
    private List<GroundItemSpawner> groundItemSpawners = new List<GroundItemSpawner>();

    [SerializeField] private float intervalToSpawnItem;

    private float currentTime;

    private void Awake()
    {
        GroundItemSpawner[] tempGroundItemSpawners = GetComponentsInChildren<GroundItemSpawner>();

        foreach (GroundItemSpawner tempGroundItemSpawner in tempGroundItemSpawners)
        {
            groundItemSpawners.Add(tempGroundItemSpawner);
        }
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
        groundItemSpawners[Random.Range(0, groundItemSpawners.Count)].Spawn(availableItems[Random.Range(0, availableItems.Count)]);
    }

    private void ResetTimer()
    {
        currentTime = intervalToSpawnItem;
    }
}
