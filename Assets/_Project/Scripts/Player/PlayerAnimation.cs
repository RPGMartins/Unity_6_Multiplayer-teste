using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void Initialize(PlayerInventory playerInventory)
    {
        playerInventory.onEquippedSlotChanged += ChangeWeaponAnimation;
    }

    private void ChangeWeaponAnimation(InventoryItem item)
    {
        switch (item.WeaponType)
        {
            case WeaponType.Flashlight:
                animator.Play("IdleFlashlight");

                break;

            case WeaponType.Pistol:
                animator.Play("IdlePistol");

                break;

            case WeaponType.Shotgun:
                animator.Play("IdleShotgun");

                break;

            case WeaponType.Rifle:
                animator.Play("IdleRifle");

                break;
        }
    }
}
