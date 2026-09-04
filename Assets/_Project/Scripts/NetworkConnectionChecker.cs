using Unity.Netcode;

public static class NetworkConnectionChecker
{
    public static bool CheckConnection()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening == true;
    }

    public static bool CheckIfHasAuthority()
    {
        return CheckConnection() && (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer);
    }
}
