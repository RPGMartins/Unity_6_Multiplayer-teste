using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControllerManager : NetworkBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameplaySceneName = "Mapa1";

    private NetworkSceneManager networkSceneManager;
    private LobbyManager lobbyManager;
    private Action<LobbyStateEnum> lobbyStateCallback;

    public override void OnNetworkSpawn()
    {
        networkSceneManager = NetworkManager.Singleton.SceneManager;

        networkSceneManager.OnLoadEventCompleted += NetworkSceneManagerOnOnLoadComplete;
        networkSceneManager.OnSynchronizeComplete += NetworkSceneManagerOnOnSynchronizeComplete;
    }

    public override void OnNetworkDespawn()
    {
        networkSceneManager.OnLoadEventCompleted -= NetworkSceneManagerOnOnLoadComplete;
        networkSceneManager.OnSynchronizeComplete -= NetworkSceneManagerOnOnSynchronizeComplete;
    }

    public void Initiliaze(Action<LobbyStateEnum> lobbyStateCallback, LobbyManager lobbyManager)
    {
        this.lobbyStateCallback = lobbyStateCallback;
        this.lobbyManager = lobbyManager;

        lobbyManager.OnSessionDisconnect += (_) => GotoMainMenuScene();
    }

    public void GotoGameplayScene()
    {
        ChangeScene(gameplaySceneName);
    }

    public void GotoMainMenuScene()
    {
        ChangeScene(mainMenuSceneName);
    }

    private void ChangeScene(string sceneName)
    {
        if (IsSpawned == true) // Has Connection
        {
            if (NetworkManager.Singleton.IsServer == true)
            {
                SceneEventProgressStatus status = networkSceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }
        }
        else
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

            LobbyStateEnum lobbyStateEnumToGo = LobbyStateEnum.OnLobby;

            if (sceneName == gameplaySceneName)
            {
                lobbyStateEnumToGo = LobbyStateEnum.OnGameplay;
            }

            lobbyStateCallback?.Invoke(lobbyStateEnumToGo);
        }
    }

    private void NetworkSceneManagerOnOnLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName == gameplaySceneName)
        {
            lobbyStateCallback?.Invoke(LobbyStateEnum.OnGameplay);

            foreach (ulong clientID in clientsCompleted)
            {
                PlayerInitializerNetwork.instance.SpawnPlayer(clientID);
            }

            PlayerInitializerNetwork.instance.FinishInitialization();
        }
        else if (sceneName == mainMenuSceneName)
        {
            lobbyStateCallback?.Invoke(LobbyStateEnum.OnLobby);
        }
    }

    private void NetworkSceneManagerOnOnSynchronizeComplete(ulong clientid)
    {
        if (SceneManager.GetActiveScene().name == mainMenuSceneName)
        {
        }
        else if (SceneManager.GetActiveScene().name == gameplaySceneName)
        {
            PlayerInitializerNetwork.instance.SpawnPlayer(clientid);
        }
    }
}
