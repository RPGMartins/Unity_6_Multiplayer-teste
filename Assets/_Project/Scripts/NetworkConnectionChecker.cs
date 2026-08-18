using Unity.Netcode;

public static class NetworkConnectionChecker
{
    public static bool CheckConnection()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening == true;
    }
}
