using Unity.Netcode;
using UnityEngine;

public static class NetworkConnectionChecker
{
    public static bool CheckConnection()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening == true;
    }
}
