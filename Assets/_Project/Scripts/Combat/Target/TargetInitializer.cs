using UnityEngine;

public class TargetInitializer : MonoBehaviour
{
    [SerializeField] private Target targetPrefab;
    [SerializeField] private int defaultLifeValue = 3;
    [SerializeField] private bool spawnTarget;

    private bool hasConnection;

    private void Awake()
    {
        hasConnection = NetworkConnectionChecker.CheckConnection();
    }

    private void Update()
    {
        if (spawnTarget == false)
        {
            return;
        }

        SpawnTarget();
        spawnTarget = false;
    }

    private void SpawnTarget()
    {
        Target target = Instantiate(targetPrefab);

        if (hasConnection == true)
        {
            if (NetworkConnectionChecker.CheckIfHasAuthority() == false)
            {
                return;
            }

            target.NetworkObject.Spawn();
        }

        target.Initialize(defaultLifeValue, () => Destroy(target.gameObject));
    }
}
