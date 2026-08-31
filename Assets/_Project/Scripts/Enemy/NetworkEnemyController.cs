using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkEnemyController : NetworkBehaviour
{
    [SerializeField] private LocalEnemyController localEnemyController;
    [SerializeField] private NetworkTarget networkTarget;

    private NetworkVariable<EnemyState> enemyState = new NetworkVariable<EnemyState>();
    private bool initialized = false;

    public override void OnNetworkSpawn()
    {
        enemyState.OnValueChanged += ChangeEnemyStateValue;
    }

    public override void OnNetworkDespawn()
    {
        enemyState.OnValueChanged -= ChangeEnemyStateValue;
    }

    public void Initialize(int life, Action destroyAction)
    {
        networkTarget.Initialize(life, destroyAction);
        localEnemyController.Initialize(life, destroyAction, true);

        initialized = true;
    }

    private void Update()
    {
        if (initialized == false)
        {
            return;
        }

        localEnemyController.EnemyCombat.MainAction(enemyState.Value, localEnemyController.EnemyDetectPlayer.Players, localEnemyController.EnemyTransform, localEnemyController.EnemyTransform.position, out EnemyState newEnemyState);

        if (newEnemyState != enemyState.Value)
        {
            enemyState.Value = newEnemyState;
        }
    }

    private void ChangeEnemyStateValue(EnemyState previousValue, EnemyState newValue)
    {
        localEnemyController.UpdateEnemyState(newValue);
    }
}
