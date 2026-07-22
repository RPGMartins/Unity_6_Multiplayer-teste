using Unity.Netcode;
using UnityEngine;

public class HelloWorldManager : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (networkManager.IsClient == false && networkManager.IsServer == false)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    private void StartButtons()
    {
        if (GUILayout.Button("Host"))
        {
            networkManager.StartHost();
        }

        if (GUILayout.Button("Client"))
        {
            networkManager.StartClient();
        }

        if (GUILayout.Button("Server"))
        {
            networkManager.StartServer();
        }
    }

    private void StatusLabels()
    {
        string mode = networkManager.IsHost ? "Host" : networkManager.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " + networkManager.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }
}

