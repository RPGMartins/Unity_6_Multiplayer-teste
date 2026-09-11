using Unity.Collections;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    private ProjectileManager projectileManager;

    private bool isConnected = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        projectileManager = ProjectileManager.Instance;
        isConnected = NetworkConnectionChecker.CheckConnection();
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

        projectileManager.Shoot(originPosition, targetPosition, Quaternion.identity, weaponID);
    }
}
