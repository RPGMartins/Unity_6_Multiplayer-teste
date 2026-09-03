using UnityEngine;
using UnityEngine.UI;

public class PlayerInLobby : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text playerNameText;
    [SerializeField] private Image readyImage;

    private PlayerData playerData;

    public PlayerData PlayerData => playerData;

    public void Initialize(PlayerData playerData)
    {
        UpdateData(playerData);
    }

    public void UpdateData(PlayerData playerData)
    {
        this.playerData = playerData;
        playerNameText.text = playerData.Name.ToString();
        readyImage.color = playerData.IsReady ? Color.green : Color.red;
    }
}
