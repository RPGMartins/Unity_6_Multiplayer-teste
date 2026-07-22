using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeText;
    [SerializeField] private Button joinButton;

    public void Initialize(Func<string,Task> hostCallback)
    {
        joinButton.onClick.AddListener(() =>
        {
            hostCallback.Invoke(joinCodeText.text);
        });
    }
}
