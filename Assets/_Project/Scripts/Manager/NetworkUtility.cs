using System;
using Unity.Netcode;
using UnityEngine;

public static class NetworkUtility
{
    public static event Action<ulong> OnNetworkLostConnection;
    public static event Action<ulong> OnClientNetworkLostConnection;

    public static void HandleConnection(NetworkManager networkManager, ConnectionEventData eventData)
    {
        switch (eventData.EventType)
        {
            case ConnectionEvent.ClientDisconnected:
                if (eventData.ClientId == networkManager.LocalClientId)
                {
                    Debug.Log("Eu perdi a conexao");
                    OnNetworkLostConnection?.Invoke(eventData.ClientId);
                    return;
                }

                if (networkManager.IsServer == true)
                {
                    Debug.Log("Alguem perdeu a conexao");
                    OnClientNetworkLostConnection?.Invoke(eventData.ClientId);

                    return;
                }

                break;
        }
    }
}
