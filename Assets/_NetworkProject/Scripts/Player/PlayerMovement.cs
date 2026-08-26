using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform transformToMove;
    [SerializeField] private Transform target;
    [SerializeField] private float velocity = 1;

    private Func<Vector2> movementAction;
    private Func<Vector3> mouseMovementAction;
    private Func<bool> shootingAction;

    private bool initialized = false;

    public Transform Target => target;

    public void Initialize(Func<Vector2> movementVector, Func<Vector3> mouseMovementVector, Func<bool> shooting)
    {
        this.movementAction = movementVector;
        this.mouseMovementAction = mouseMovementVector;
        this.shootingAction = shooting;
        initialized = true;
    }

    private void Update()
    {
        if (initialized == false)
        {
            return;
        }

        Vector3 movement = movementAction.Invoke();
        transformToMove.position += movement * (velocity * Time.deltaTime);
        target.position = mouseMovementAction.Invoke();

        if (shootingAction.Invoke() == true)
        {
            WeaponManager.Instance.Shoot(transformToMove.position, target.position);
        }

        RotateTowardsTarget();
    }

    private void RotateTowardsTarget()
    {
        Vector2 direction = target.position - transformToMove.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transformToMove.rotation = Quaternion.Euler(0, 0, angle);
    }
}
