using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform transformToMove;
    [SerializeField] private float velocity = 1;

    private Func<Vector2> movementAction;
    private Func<float> angleToRotate;
    private bool initialized = false;

    public void Initialize(Func<Vector2> movementVector, Func<float> angleToRotate)
    {
        this.movementAction = movementVector;
        this.angleToRotate = angleToRotate;
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
        transformToMove.rotation = Quaternion.Euler(0, 0, angleToRotate.Invoke());
    }
}
