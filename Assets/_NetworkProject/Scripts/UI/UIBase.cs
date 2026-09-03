using UnityEngine;

public class UIBase : MonoBehaviour
{
    public void CloseScreen()
    {
        gameObject.SetActive(false);
    }

    public void OpenScreen()
    {
        gameObject.SetActive(true);
    }
}
