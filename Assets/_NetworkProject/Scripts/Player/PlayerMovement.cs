using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform transformToMove;
    [SerializeField] private Transform target;
    [SerializeField] private float velocity = 1;

    private Func<Vector2> movementAction;
    private Func<Vector3> mouseMovementAction;

    public Transform Target => target;

    public void Initialize(Func<Vector2> movementVector, Func<Vector3> mouseMovementVector)
    {
        this.movementAction = movementVector;
        this.mouseMovementAction = mouseMovementVector;
    }

    private void Update()
    {
        Vector3 movement = movementAction.Invoke();
        transformToMove.position += movement * (velocity * Time.deltaTime);
        target.position = mouseMovementAction.Invoke();
        RotateTowardsTarget();
    }

    private void RotateTowardsTarget()
    {
        Vector2 direction = target.position - transformToMove.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transformToMove.rotation = Quaternion.Euler(0, 0, angle);
    }
}
