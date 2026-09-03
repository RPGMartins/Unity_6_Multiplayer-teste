using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;

public class ErrorScreenUI : UIBase
{
    [SerializeField] private TMP_Text errorText;

    public void Initialize(SessionError sessionError)
    {
        this.errorText.text = sessionError.ToString();
    }
}
