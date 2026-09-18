using TMPro;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyScreen : UIBase
{
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_InputField lobbyPasswordInputField;
    [SerializeField] private TMP_InputField lobbyQuantityInputField;
    [SerializeField] private Toggle isPrivateToggle;

    public void CreateLobby()
    {
        int.TryParse(lobbyQuantityInputField.text, out int playerQuantity);
        SessionOptions options = ManageNetworkUI.CreateSessionOptions(lobbyNameInputField.text, isPrivateToggle.isOn, playerQuantity, lobbyPasswordInputField.text).WithRelayNetwork();

        if (NetworkManager.Singleton.IsConnectedClient == false)
        {
            SessionManager.Instance.CreateSession(options);
        }
        else
        {
            SessionManager.Instance.UpdateLobbyData(isPrivateToggle.isOn, lobbyNameInputField.text);
        }
    }
}
