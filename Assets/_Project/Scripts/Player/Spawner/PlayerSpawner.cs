using System;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour, IPlayerSpawner
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private PlayerSpawnerController playerSpawnerController;

    public IPlayerController SpawnPlayer(ulong clientID)
    {
        return playerSpawnerController.SpawnPlayer(playerPrefab);
    }
}
