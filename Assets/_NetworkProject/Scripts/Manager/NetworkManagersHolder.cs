using UnityEngine;

public class NetworkManagersHolder : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(this);
    }
}
