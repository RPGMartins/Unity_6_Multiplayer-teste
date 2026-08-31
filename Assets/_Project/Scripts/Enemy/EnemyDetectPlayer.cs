using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectPlayer : MonoBehaviour
{
    [SerializeField] private List<PlayerHitbox> players = new List<PlayerHitbox>();
    public List<PlayerHitbox> Players => players;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHitbox playerHitbox = other.GetComponent<PlayerHitbox>();

        if (playerHitbox == null)
        {
            return;
        }

        players.Add(playerHitbox);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerHitbox playerHitbox = other.GetComponent<PlayerHitbox>();

        if (playerHitbox == null)
        {
            return;
        }

        players.Remove(playerHitbox);
    }
}
