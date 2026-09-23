using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControllerManager : NetworkBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameplaySceneName = "Mapa1";

    private NetworkSceneManager networkSceneManager;
    private SessionManager sessionManager;
    public static SceneControllerManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        this.sessionManager = SessionManager.Instance;
    }

    public override void OnNetworkSpawn()
    {
        networkSceneManager = NetworkManager.Singleton.SceneManager;

        networkSceneManager.OnLoadEventCompleted += OnLoadComplete;
        networkSceneManager.OnSynchronizeComplete += OnSynchronizeComplete;
    }

    public override void OnNetworkDespawn()
    {
        networkSceneManager.OnLoadEventCompleted -= OnLoadComplete;
        networkSceneManager.OnSynchronizeComplete -= OnSynchronizeComplete;
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
        if (IsSpawned == true) // Has Connection //TODO usar gameSession
        {
            if (NetworkManager.Singleton.IsServer == true)
            {
                SceneEventProgressStatus status = networkSceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }
        }
        else
        {
            LoadSceneMode loadSceneMode = LoadSceneMode.Single;
            SceneManager.LoadScene(sceneName, loadSceneMode);
            List<PlayerData> playersOnSession = sessionManager.CurrentSession.CurrentPlayersOnSession;
            List<ulong> clientIds = new List<ulong>();

            foreach (PlayerData player in playersOnSession)
            {
                clientIds.Add(player.ClientId);
            }

            OnLoadComplete(sceneName, loadSceneMode, clientIds, null);
        }
    }

    private void OnLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sessionManager.CurrentSession.HasHostAuthority() == false)
        {
            return;
        }

        List<FixedString64Bytes> playerIdList = new List<FixedString64Bytes>();

        foreach (ulong clientID in clientsCompleted)
        {
            FixedString64Bytes playerAuthenticateID = GetPlayerAuthenticateIDByClientID(clientID);

            if (playerAuthenticateID.Value != null)
            {
                playerIdList.Add(playerAuthenticateID);
            }
        }

        StartCoroutine(WaitPlayerInitializerBeingInitialized(() => OnLoadComplete(sceneName, playerIdList)));
    }

    private IEnumerator WaitPlayerInitializerBeingInitialized(Action callback)
    {
        yield return new WaitUntil(() => PlayerInitializer.instance != null);

        callback.Invoke();
    }

    private void OnLoadComplete(string sceneName, List<FixedString64Bytes> playerIDsList)
    {
        if (sceneName == gameplaySceneName)
        {
            PlayerInitializer.instance.PreparePlayersToBeSpawned(playerIDsList.ToArray());
        }
        else if (sceneName == mainMenuSceneName)
        {
        }
    }

    private void OnSynchronizeComplete(ulong clientID) //Quando cliente sincroniza com a sessao
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
