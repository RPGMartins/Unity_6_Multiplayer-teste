using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbySlot : MonoBehaviour
{
    [SerializeField] private TMP_Text playerIDText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Image playerReady;

    private FixedString64Bytes currentID;
    public FixedString64Bytes CurrentID => currentID;

    public void Initialize(PlayerData playerData)
    {
        currentID = playerData.AuthenticationId;
        UpdateData(playerData);
    }

    public void UpdateData(PlayerData playerData)
    {
        playerIDText.text = playerData.AuthenticationId.Value;
        playerNameText.text = playerData.Name.Value;
        SetReady(playerData.IsReady);
    }

    public void SetReady(bool isReady)
    {
        if (isReady == true)
        {
            playerReady.color = Color.green;
        }
        else
        {
            playerReady.color = Color.red;
        }
    }
}
