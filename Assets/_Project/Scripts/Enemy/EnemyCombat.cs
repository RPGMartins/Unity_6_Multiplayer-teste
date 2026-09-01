using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [SerializeField] private float shootCooldownMax;
    private PlayerHitbox nearPlayer;
    private float currentShootColldown;

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

    private bool FollowPlayer(Transform enemyTransform, Vector2 playerPosition)
    {
        bool isNear = CheckIfIsNear(enemyTransform, playerPosition);

        if (isNear == true)
        {
            return true;
        }

        enemyTransform.position = Vector3.MoveTowards(enemyTransform.position, playerPosition, 1 * Time.deltaTime);

        return false;
    }

    private bool CheckIfIsNear(Transform enemyTransform, Vector2 playerPosition)
    {
        if (Vector2.Distance(enemyTransform.position, playerPosition) <= 2)
        {
            return true;
        }

        return false;
    }
}

public enum EnemyState
{
    Idle, Following
}
