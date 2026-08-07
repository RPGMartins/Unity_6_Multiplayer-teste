using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    public event Action<IAuthenticationService> OnAuthenticatedSucceed;
    public event Action OnAuthenticatedFailed;

    private IAuthenticationService authenticationService;

    [SerializeField] private bool updateName;

    public string AuthenticationID => authenticationService.PlayerId;
    public string PlayerName => authenticationService.PlayerName;

    private void Awake()
    {
        InitializeAsync();
    }

    private void Update()
    {
        if (updateName)
        {
            updateName = false;
            UpdatePlayerName("HSADJLGTH");
        }
    }

    private async void InitializeAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();
            authenticationService = AuthenticationService.Instance;
            await authenticationService.SignInAnonymouslyAsync();
            OnAuthenticatedSucceed?.Invoke(authenticationService);
        }
        catch (Exception e)
        {
            OnAuthenticatedFailed?.Invoke();
            Debug.LogException(e);
        }
    }

    private async Task UpdatePlayerName(string playerName)
    {
        await authenticationService.UpdatePlayerNameAsync(playerName);
        Debug.Log($"Player {playerName} updated is {PlayerName}");
    }
}
