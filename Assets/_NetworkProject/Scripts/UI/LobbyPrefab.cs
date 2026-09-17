using System.Collections.Generic;
using Unity.Services.Multiplayer;
using UnityEngine;

public class LobbyPrefab : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text playersText;
    [SerializeField] private TMPro.TMP_Text extraText;

    private ISessionInfo currentSessionInfo;

    public void Inititlaize(ISessionInfo sessionInfo)
    {
        this.currentSessionInfo = sessionInfo;
        playersText.text = $"A vailable slots: {sessionInfo.AvailableSlots.ToString()}";
    }

    public void JoinLobby()
    {
        SessionManager.Instance.JoinSessionByID(currentSessionInfo.Id);
    }
}
