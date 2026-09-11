using System;
using UnityEngine;

public class LocalEnemyController : MonoBehaviour
{
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private EnemyAnimator enemyAnimator;
    [SerializeField] private Target target;
    [SerializeField] private EnemyDetectPlayer enemyDetectPlayer;
    [SerializeField] private EnemyCombat enemyCombat;

    private bool initialized = false;
    private EnemyState enemyState;

    public Transform EnemyTransform => enemyTransform;
    public EnemyAnimator EnemyAnimator => enemyAnimator;
    public EnemyDetectPlayer EnemyDetectPlayer => enemyDetectPlayer;
    public EnemyCombat EnemyCombat => enemyCombat;

    public void Initialize(int life, Action destroyAction, bool network)
    {
        if (network == false)
        {
            target.Initialize(life, destroyAction);
            initialized = true;
        }
    }

    private void Update()
    {
        if (initialized == false)
        {
            return;
        }

        EnemyCombat.MainAction(enemyState, EnemyDetectPlayer.Players, enemyTransform, EnemyTransform.position, out EnemyState newEnemyState);

        if (newEnemyState != enemyState)
        {
            UpdateEnemyState(newEnemyState);
        }
    }

    public void UpdateEnemyState(EnemyState enemyState)
    {
        this.enemyState = enemyState;
    }
}
