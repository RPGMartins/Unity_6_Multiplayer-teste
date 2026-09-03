using System;
using TMPro;
using UnityEngine;

public class JoinLobbyScreen : UIBase
{
    [SerializeField] private TMP_InputField lobbyCodeInputField;

    private void Awake()
    {
        lobbyCodeInputField.onValueChanged.AddListener(TextUpdate);
    }

    public void JoinLobby()
    {
        LobbyManager.Instance.JoinSessionByCode(lobbyCodeInputField.text);
    }

    private void TextUpdate(string text)
    {
        text = text.Trim();
        text = text.ToUpperInvariant();
        lobbyCodeInputField.text = text;
    }
}
