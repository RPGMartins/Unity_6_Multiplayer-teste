using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void ChangeWeaponAnimation(WeaponTypeEnum weaponTypeEnum)
    {
        switch (weaponTypeEnum)
        {
            case WeaponTypeEnum.Flashlight:
                animator.Play("IdleFlashlight");

                break;

            case WeaponTypeEnum.Pistol:
                animator.Play("IdlePistol");

                break;

            case WeaponTypeEnum.Shotgun:
                animator.Play("IdleShotgun");

                break;

            case WeaponTypeEnum.Rifle:
                animator.Play("IdleRifle");

                break;
        }
    }
}
