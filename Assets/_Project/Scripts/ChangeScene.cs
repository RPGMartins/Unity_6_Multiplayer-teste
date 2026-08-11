using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [SerializeField] private string sceneName;

    void Start()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
