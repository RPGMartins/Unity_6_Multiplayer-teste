using System.Collections.Generic;
using Unity.Services.Multiplayer;
using UnityEngine;

public class LobbiesUI : UIBase
{
    [SerializeField] private RectTransform lobbieLayoutGroup;
    [SerializeField] private LobbyPrefab lobbyPrefab;

    private List<LobbyPrefab> lobbies = new List<LobbyPrefab>();

    public void Initialize()
    {
    }

    public async void RefreshLobbies()
    {
        for (int i = lobbies.Count - 1; i >= 0; i--)
        {
            Destroy(lobbies[i].gameObject);
            lobbies.RemoveAt(i);
        }

        QuerySessionsOptions querySessionsOptions = new QuerySessionsOptions();
        QuerySessionsResults querySessionsResults = await MultiplayerService.Instance.QuerySessionsAsync(querySessionsOptions);

        foreach (ISessionInfo sessionInfo in querySessionsResults.Sessions)
        {
            LobbyPrefab tempLobbyPrefab = Instantiate(lobbyPrefab, lobbieLayoutGroup);
            lobbies.Add(tempLobbyPrefab);
            tempLobbyPrefab.Inititlaize(sessionInfo);
        }
    }
}
