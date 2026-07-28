using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeText;
    [SerializeField] private Button joinButton;

    public void Initialize(Func<string,Task> clientCallback)
    {
        joinButton.onClick.AddListener(() =>
        {
            clientCallback.Invoke(joinCodeText.text);
        });
    }
}
