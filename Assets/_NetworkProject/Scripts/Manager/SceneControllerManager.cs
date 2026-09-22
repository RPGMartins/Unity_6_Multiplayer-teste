using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControllerManager : NetworkBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameplaySceneName = "Mapa1";

    private NetworkSceneManager networkSceneManager;
    private SessionManager sessionManager;
    private Action<SessionStateEnum> lobbyStateCallback;
    public static SceneControllerManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

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

    public void Initiliaze(Action<SessionStateEnum> lobbyStateCallback, SessionManager sessionManager)
    {
        this.lobbyStateCallback = lobbyStateCallback;
        this.sessionManager = sessionManager;

        sessionManager.OnSessionDisconnect += (_) => GotoMainMenuScene();
    }

    [ContextMenu("GotoGameplayScene")]
    public void GotoGameplayScene()
    {
        ChangeScene(gameplaySceneName);
    }

    [ContextMenu("GotoMainMenuScene")]
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

            SessionStateEnum sessionStateEnumToGo = SessionStateEnum.OnLobby;

            if (sceneName == gameplaySceneName)
            {
                sessionStateEnumToGo = SessionStateEnum.OnGameplay;
            }

            lobbyStateCallback?.Invoke(sessionStateEnumToGo);
        }
    }

    private void NetworkSceneManagerOnOnLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (NetworkManager.Singleton.IsServer == false)
        {
            return;
        }

        if (sceneName == gameplaySceneName) // criar uma classe GameplayInitializer, em que vai passar a lista de clients, e ela vai incializar tudo certinho
        {
            lobbyStateCallback?.Invoke(SessionStateEnum.OnGameplay);
            List<FixedString64Bytes> playerIds = new List<FixedString64Bytes>();

            foreach (ulong clientID in clientsCompleted)
            {
                FixedString64Bytes playerAuthenticateID = GetPlayerAuthenticateIDByClientID(clientID);

                if (playerAuthenticateID.Value != null)
                {
                    playerIds.Add(playerAuthenticateID);
                }
            }

            PlayerInitializer.instance.PreparePlayersToBeSpawned(playerIds.ToArray());
        }
        else if (sceneName == mainMenuSceneName)
        {
            lobbyStateCallback?.Invoke(SessionStateEnum.OnLobby);
        }
    }

    private void NetworkSceneManagerOnOnSynchronizeComplete(ulong clientID) //Quando cliente sincroniza com a sessao
    {
        if (NetworkManager.Singleton.IsServer == true)
        {
            return;
        }

        if (SceneManager.GetActiveScene().name == mainMenuSceneName)
        {
        }
        else if (SceneManager.GetActiveScene().name == gameplaySceneName)
        {
            List<FixedString64Bytes> playerIds = new List<FixedString64Bytes>();

            FixedString64Bytes playerAuthenticateID = GetPlayerAuthenticateIDByClientID(clientID);

            if (playerAuthenticateID != null)
            {
                playerIds.Add(playerAuthenticateID);
            }

            PlayerInitializer.instance.PreparePlayersToBeSpawned(playerIds.ToArray());
        }
    }

    private FixedString64Bytes GetPlayerAuthenticateIDByClientID(ulong clientID)
    {
        bool hasPlayer = sessionManager.CurrentSession.GetPlayerDataByClientID(clientID.ToString(), out PlayerData playerData);

        if (hasPlayer == false)
        {
            return null;
        }

        return playerData.AuthenticationID;
    }
}
