using System;
using System.Collections;
using UnityEngine;

public class NetworkManagersHolder : MonoBehaviour
{
    [SerializeField] private AuthenticationManager authenticationManager;
    [SerializeField] private LobbyManager lobbyManager;
    [SerializeField] private SceneControllerManager sceneControllerManager;

    private void Awake()
    {
        authenticationManager.Initialize();
        StartCoroutine(OnAwake());
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    private IEnumerator OnAwake()
    {
        yield return null;

        lobbyManager.Initialize();
    }
}
