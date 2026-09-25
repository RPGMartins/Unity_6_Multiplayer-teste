using System;
using Unity.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Transform aimTarget;
    [SerializeField] private Transform parentTransform;

    private Func<Vector3> mouseMovementAction;
    private Func<bool> shootingAction;
    private FixedString64Bytes playerID;
    private bool initialized = false;

    public void Initialize(FixedString64Bytes playerID, Func<Vector3> mouseMovementVector, Func<bool> shooting)
    {
        this.playerID = playerID;
        this.shootingAction = shooting;
        this.mouseMovementAction = mouseMovementVector;
        initialized = true;
    }

    private void Update()
    {
        if (initialized == false)
        {
            return;
        }

        aimTarget.position = mouseMovementAction.Invoke();

        if (shootingAction.Invoke() == true)
        {
            WeaponManager.Instance.Shoot(playerID, GetWeaponShootingOrigin(), aimTarget.position);
        }
    }

    private Vector2 GetWeaponShootingOrigin()
    {
        return parentTransform.position;
    }

    public float GetAngleToRotate()
    {
        Vector2 direction = aimTarget.position - parentTransform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        return angle;
    }

    public void EnableDisableAim(bool value)
    {
        aimTarget.gameObject.SetActive(value);
    }
}
