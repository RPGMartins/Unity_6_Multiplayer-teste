using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    public event Action<IAuthenticationService> OnAuthenticatedSucceed;
    public event Action OnAuthenticatedFailed;

    private IAuthenticationService authenticationService;

    public string PlayerID => authenticationService.PlayerId;
    public string PlayerName => authenticationService.PlayerName;

    private void Awake()
    {
        InitializeAsync();
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
}
