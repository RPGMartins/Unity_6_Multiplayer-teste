using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    private PlayerHitbox nearPlayer;

    public void MainAction(EnemyState enemyState, List<PlayerHitbox> players, Transform enemyTransform, Vector2 enemyPosition, out EnemyState newEnemyState)
    {
        newEnemyState = enemyState;

        switch (enemyState)
        {
            case EnemyState.Idle:

                nearPlayer = DetectNearPlayer(players, enemyPosition);

                if (nearPlayer != null)
                {
                    newEnemyState = EnemyState.Following;
                }

                break;

            case EnemyState.Following:

                nearPlayer = DetectNearPlayer(players, enemyPosition);

                if (nearPlayer == null)
                {
                    newEnemyState = EnemyState.Idle;

                    return;
                }

                FollowPlayer(enemyTransform, nearPlayer.PlayerTransform.position);

                break;

            case EnemyState.Attacking:

                break;
        }
    }

    private PlayerHitbox DetectNearPlayer(List<PlayerHitbox> playersHitboxes, Vector2 enemyPosition)
    {
        float distance = Single.MaxValue;
        PlayerHitbox playerToFollow = null;

        foreach (PlayerHitbox playerHitbox in playersHitboxes)
        {
            float tempDistance = Vector2.Distance(playerHitbox.PlayerTransform.position, enemyPosition);

            if (tempDistance < distance)
            {
                distance = tempDistance;
                playerToFollow = playerHitbox;
            }
        }

        return playerToFollow;
    }

    private void FollowPlayer(Transform enemyTransform, Vector2 playerPosition)
    {
        enemyTransform.position = Vector3.MoveTowards(enemyTransform.position, playerPosition, 1 * Time.deltaTime);
    }
}

public enum EnemyState
{
    Idle, Following, Attacking
}
