using Unity.Collections;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    private NetworkProjectileManager networkProjectileManager;
    private ProjectileManager projectileManager;

    private bool isConnected = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        projectileManager = ProjectileManager.Instance;
        networkProjectileManager = NetworkProjectileManager.Instance;

        isConnected = NetworkConnectionChecker.CheckConnection();

        if (isConnected == true)
        {
            projectileManager.OnProjectileExpired += networkProjectileManager.ReturnProjectile;
        }
        else
        {
            projectileManager.OnProjectileExpired += projectileManager.ReturnProjectile;
        }
    }

    public void Shoot(FixedString64Bytes playerID, Vector3 originPosition, Vector3 targetPosition)
    {
        //Validar arma/cooldown etc

        FixedString64Bytes weaponID = InventoryManager.Instance.GetCurrentPlayerEquipedItem(playerID);

        if (weaponID.IsEmpty == true)
        {
            Debug.LogWarning("Tentando atirar sem ter arma equipada");

            return;
        }

        if (isConnected == true)
        {
            networkProjectileManager.Shoot(originPosition, targetPosition, Quaternion.identity, weaponID);
        }
        else
        {
            projectileManager.Shoot(originPosition, targetPosition, Quaternion.identity, weaponID);
        }
    }
}
