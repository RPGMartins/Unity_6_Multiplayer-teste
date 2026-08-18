using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void Initialize(PlayerLocalInventoryController playerLocalInventoryController)
    {
        playerLocalInventoryController.onEquippedSlotChanged += ChangeWeaponAnimation;
    }

    private void ChangeWeaponAnimation(WeaponItem item)
    {
        switch (item.WeaponTypeEnum)
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
