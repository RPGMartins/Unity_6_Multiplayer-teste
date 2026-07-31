using System;
using Unity.Netcode;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    public event Action<LobbyData> OnLobbyUpdated;
    public event Action<PlayerData, NetworkListEvent<PlayerData>.EventType> OnPlayersUpdated;

    public void Initialize(NetworkVariable<LobbyData> lobbyDataNetworkVariable, NetworkList<PlayerData> playerDataList)
    {
        lobbyDataNetworkVariable.OnValueChanged += LobbyDataValueChanged;
        playerDataList.OnListChanged += LobbyPlayersDataChanged;
    }

    private void LobbyDataValueChanged(LobbyData previousValue, LobbyData newValue)
    {
        OnLobbyUpdated?.Invoke(newValue);
    }

    private void LobbyPlayersDataChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayersUpdated?.Invoke(changeEvent.Value, changeEvent.Type);
    }
}
