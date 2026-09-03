using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyViwerUI : UIBase
{
    [SerializeField] private TMPro.TMP_Text lobbyNameText;
    [SerializeField] private TMPro.TMP_Text lobbyCodeText;
    [SerializeField] private PlayerInLobby playerInLobbyPrefab;
    [SerializeField] private LayoutGroup lobbyViwerUILayoutGroup;
    [SerializeField] private Button startMatchButton;

    private List<PlayerInLobby> playerInLobbyList = new List<PlayerInLobby>();

    private LobbyManager lobbyManager;

    private void Start()
    {
        lobbyManager = LobbyManager.Instance;
        lobbyManager.OnLobbyUpdated += UpdateLobbyData;
        lobbyManager.OnPlayersUpdated += UpdatePlayerData;
    }

    private void OnDestroy()
    {
        if (lobbyManager == null)
        {
            return;
        }

        lobbyManager.OnLobbyUpdated -= UpdateLobbyData;
        lobbyManager.OnPlayersUpdated -= UpdatePlayerData;
    }

    public void Initialize()
    {
        startMatchButton.gameObject.SetActive(NetworkManager.Singleton.IsHost);

        foreach (PlayerInLobby playerInLobby in playerInLobbyList)
        {
            RemovePlayer(playerInLobby);
        }

        foreach (PlayerData playerData in LobbyManager.Instance.GetLobbyPlayers())
        {
            AddPlayer(playerData);
        }

        UpdateLobbyData(LobbyManager.Instance.GetLobbyData());
    }

    private void UpdateLobbyData(LobbyData lobbyData)
    {
        lobbyNameText.text = lobbyData.LobbyName.Value;
        lobbyCodeText.text = lobbyData.LobbyCode.Value;
    }

    private void UpdatePlayerData(PlayerData playerData, NetworkListEvent<PlayerData>.EventType playerEventType)
    {
        if (playerEventType == NetworkListEvent<PlayerData>.EventType.Add || playerEventType == NetworkListEvent<PlayerData>.EventType.Insert)
        {
            AddPlayer(playerData);
        }
        else if (playerEventType == NetworkListEvent<PlayerData>.EventType.Remove || playerEventType == NetworkListEvent<PlayerData>.EventType.RemoveAt)
        {
            foreach (PlayerInLobby playerInLobby in playerInLobbyList)
            {
                if (playerInLobby.PlayerData.AuthenticationID == playerData.AuthenticationID)
                {
                    RemovePlayer(playerInLobby);

                    return;
                }
            }
        }
        else if (playerEventType == NetworkListEvent<PlayerData>.EventType.Clear)
        {
            for (int i = playerInLobbyList.Count - 1; i >= 0; i--)
            {
                RemovePlayer(playerInLobbyList[i]);
            }
        }
        else if (playerEventType == NetworkListEvent<PlayerData>.EventType.Value)
        {
            UpdatePlayer(playerData);
        }
    }

    private void AddPlayer(PlayerData playerData)
    {
        PlayerInLobby playerInLobby = Instantiate(playerInLobbyPrefab, lobbyViwerUILayoutGroup.transform);
        playerInLobby.Initialize(playerData);
        playerInLobbyList.Add(playerInLobby);
    }

    private void RemovePlayer(PlayerInLobby playerInLobby)
    {
        playerInLobbyList.Remove(playerInLobby);
        Destroy(playerInLobby.gameObject);
    }

    private void UpdatePlayer(PlayerData playerData)
    {
        for (int i = playerInLobbyList.Count - 1; i >= 0; i--)
        {
            if (playerInLobbyList[i].PlayerData.AuthenticationID == playerData.AuthenticationID)
            {
                playerInLobbyList[i].UpdateData(playerData);
            }
        }
    }

    public void StartMatch()
    {
        SceneControllerManager.Instance.GotoGameplayScene();
    }

    public void ToggleReady()
    {
        lobbyManager.ToggleCurrentPlayerReady();
    }

    public async void LeaveLobby()
    {
        await lobbyManager.LeaveLobby();
    }
}
