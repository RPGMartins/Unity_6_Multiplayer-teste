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

    private SessionManager sessionManager;

    private void Start()
    {
        sessionManager = SessionManager.Instance;
        sessionManager.CurrentSession.OnSessionUpdated += UpdateSessionData;
        sessionManager.CurrentSession.OnPlayersUpdated += UpdatePlayerData;
    }

    private void OnDestroy()
    {
        if (sessionManager == null)
        {
            return;
        }

        sessionManager.CurrentSession.OnSessionUpdated -= UpdateSessionData;
        sessionManager.CurrentSession.OnPlayersUpdated -= UpdatePlayerData;
    }

    public void Initialize(GameSession gameSession)
    {
        if (gameSession.IsOnline == false)
        {
            startMatchButton.gameObject.SetActive(true);
        }
        else
        {
            startMatchButton.gameObject.SetActive(gameSession.HasHostAuthority());
        }

        for (int i = playerInLobbyList.Count - 1; i >= 0; i--)
        {
            RemovePlayer(playerInLobbyList[i]);
        }

        foreach (PlayerData playerData in gameSession.CurrentPlayersOnSession)
        {
            AddPlayer(playerData);
        }

        UpdateSessionData(gameSession.SessionData);
    }

    private void UpdateSessionData(SessionData sessionData)
    {
        lobbyNameText.text = sessionData.LobbyName.Value;
        lobbyCodeText.text = sessionData.LobbyCode.Value;
    }

    private void UpdatePlayerData(PlayerData playerData, NetworkListEvent<PlayerData>.EventType playerEventType)
    {
        if (playerEventType == NetworkListEvent<PlayerData>.EventType.Add || playerEventType == NetworkListEvent<PlayerData>.EventType.Insert)
        {
            AddPlayer(playerData);
        }
        else if (playerEventType == NetworkListEvent<PlayerData>.EventType.Remove || playerEventType == NetworkListEvent<PlayerData>.EventType.RemoveAt)
        {
            for (int i = playerInLobbyList.Count - 1; i >= 0; i--)
            {
                PlayerInLobby playerInLobby = playerInLobbyList[i];

                if (playerInLobby.PlayerData.AuthenticationID != playerData.AuthenticationID)
                {
                    continue;
                }

                RemovePlayer(playerInLobby);

                return;
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
        sessionManager.CurrentSession.ToggleCurrentPlayerReady();
    }

    public async void LeaveLobby()
    {
        await sessionManager.LeaveLobby();
    }
}
