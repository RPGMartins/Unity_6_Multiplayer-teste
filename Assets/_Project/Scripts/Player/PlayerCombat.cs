using System;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Transform aimTarget;
    [SerializeField] private Transform parentTransform;

    private Func<Vector3> mouseMovementAction;
    private Func<bool> shootingAction;

    public void Initialize(Func<Vector3> mouseMovementVector, Func<bool> shooting)
    {
        this.shootingAction = shooting;
        this.mouseMovementAction = mouseMovementVector;
    }

    private void Update()
    {
        aimTarget.position = mouseMovementAction.Invoke();

        if (shootingAction.Invoke() == true)
        {
            WeaponManager.Instance.Shoot(GetWeaponShootingOrigin(), aimTarget.position);
        }
    }

    private Vector2 GetWeaponShootingOrigin()
    {
        return Vector2.zero;
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
