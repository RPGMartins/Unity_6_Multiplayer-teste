using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private Target target;
    [SerializeField] private EnemyDetectPlayer enemyDetectPlayer;
    [SerializeField] private EnemyCombat enemyCombat;

    private bool initialized = false;
    private EnemyState enemyState;

    public EnemyDetectPlayer EnemyDetectPlayer => enemyDetectPlayer;
    public EnemyCombat EnemyCombat => enemyCombat;

    public void Initialize(int life, Action destroyAction)
    {
        if (NetworkConnectionChecker.CheckConnection() == true && NetworkConnectionChecker.CheckIfHasAuthority() == false)
        {
            return;
        }

        target.Initialize(life, destroyAction);
        initialized = true;
    }

    private void Update()
    {
        if (initialized == false)
        {
            return;
        }

        EnemyCombat.MainAction(enemyState, EnemyDetectPlayer.Players, transform, transform.position, out EnemyState newEnemyState);

        if (newEnemyState != enemyState)
        {
            UpdateEnemyState(newEnemyState);
        }
    }

    private void UpdateEnemyState(EnemyState enemyState)
    {
        this.enemyState = enemyState;
    }
}
