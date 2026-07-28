using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    private LobbyManager lobbyManager;

    [SerializeField] private PlayerLobbySlot playerLobbySlot;
    [SerializeField] private RectTransform playerLobbyHolder;

    [SerializeField] private TMPro.TMP_Text lobbyName;
    [SerializeField] private TMPro.TMP_Text lobbyCode;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private List<PlayerLobbySlot> listPlayerLobbySlot = new List<PlayerLobbySlot>();

    private void Awake()
    {
        lobbyManager = FindFirstObjectByType<LobbyManager>();

        lobbyManager.onLobbyUpdated += LobbyManagerOnonLobbyUpdated;
        lobbyManager.onPlayersUpdated += LobbyManagerOnonPlayersUpdated;

        readyButton.onClick.AddListener(ClickReadyButton);
    }

    private void ClickReadyButton()
    {
        lobbyManager.ToggleCurrentPlayerReady();
    }

    private void LobbyManagerOnonLobbyUpdated(LobbyData lobbyData)
    {
        lobbyCode.text = lobbyData.LobbyCode.Value;
        lobbyName.text = lobbyData.LobbyName.Value;
    }

    private void LobbyManagerOnonPlayersUpdated(PlayerData playerData, NetworkListEvent<PlayerData>.EventType eventType)
    {
        switch (eventType)
        {
            case NetworkListEvent<PlayerData>.EventType.Add:
                AddPlayerLobbySlot(playerData);

                break;

            case NetworkListEvent<PlayerData>.EventType.Value:
                UpdatePlayerLobbySlot(playerData);

                break;

            case NetworkListEvent<PlayerData>.EventType.RemoveAt:
                ClearPlayerLobbySlot(playerData);

                break;

            case NetworkListEvent<PlayerData>.EventType.Clear:
                ClearAllPlayerLobbySlot();

                return;
        }
    }

    private PlayerLobbySlot CreatePlayerSlot(PlayerData playerData)
    {
        PlayerLobbySlot playerSlot = Instantiate(playerLobbySlot, playerLobbyHolder);
        playerSlot.Initialize(playerData);

        return playerSlot;
    }

    public void ResetVariables()
    {
        lobbyCode.text = string.Empty;
        lobbyName.text = string.Empty;

        ClearAllPlayerLobbySlot();
    }

    private void UpdatePlayerLobbySlot(PlayerData playerData)
    {
        foreach (PlayerLobbySlot lobbySlot in listPlayerLobbySlot)
        {
            if (playerData.AuthenticationId.Equals(lobbySlot.CurrentID) == true)
            {
                lobbySlot.UpdateData(playerData);

                return;
            }
        }
    }

    private void AddPlayerLobbySlot(PlayerData playerData)
    {
        PlayerLobbySlot playerSlot = CreatePlayerSlot(playerData);
        listPlayerLobbySlot.Add(playerSlot);
    }

    private void ClearPlayerLobbySlot(PlayerData playerData)
    {
        for (int i = listPlayerLobbySlot.Count - 1; i >= 0; i--)
        {
            if (listPlayerLobbySlot[i].CurrentID.Equals(playerData.AuthenticationId) == true)
            {
                RemovePlayerLobbySlot(listPlayerLobbySlot[i]);
            }
        }
    }

    private void ClearAllPlayerLobbySlot()
    {
        for (int i = listPlayerLobbySlot.Count - 1; i >= 0; i--)
        {
            PlayerLobbySlot playerLobby = listPlayerLobbySlot[i];
            RemovePlayerLobbySlot(playerLobby);
        }
    }

    private void RemovePlayerLobbySlot(PlayerLobbySlot playerSlot)
    {
        Destroy(playerSlot.gameObject);
        listPlayerLobbySlot.Remove(playerSlot);
    }
}
