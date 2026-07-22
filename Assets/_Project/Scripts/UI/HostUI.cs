using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class HostUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField sessionNameText;
    [SerializeField] private TMP_InputField playersQuantityText;
    [SerializeField] private TMP_InputField passWordText;
    [SerializeField] private Toggle privateToggle;
    [SerializeField] private Button hostButton;

    public void Initialize(Func<SessionOptions,Task> hostCallback)
    {
        hostButton.onClick.AddListener(() =>
        {
            SessionOptions options = new SessionOptions
            {
                MaxPlayers = Convert.ToInt32(playersQuantityText.text),
                IsPrivate = privateToggle.isOn,
                Name = sessionNameText.text,
                Password = string.IsNullOrWhiteSpace(passWordText.text) ? null : passWordText.text, 
            }.WithRelayNetwork();
            
            hostCallback.Invoke(options);
        });
    }
}
