using System;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    public event Action<IAuthenticationService> OnAuthenticatedSucceed;
    public event Action OnAuthenticatedFailed;

    private IAuthenticationService authenticationService;

    [SerializeField] private bool updateName;

    public FixedString64Bytes AuthenticationID => authenticationService.PlayerId;
    public string PlayerName => authenticationService.PlayerName;

    public void Start()
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

    private async Task UpdatePlayerName(string playerName)
    {
        await authenticationService.UpdatePlayerNameAsync(playerName);
        Debug.Log($"Player {playerName} updated is {PlayerName}");
    }
}
