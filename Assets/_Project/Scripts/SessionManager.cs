using UnityEngine;
using Unity.Services.Core;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

public class SessionManager : MonoBehaviour
{
    [SerializeField] HostUI privateHostUI;

    [Header("Client Session")]
    [SerializeField] private string joinCode;
    
	private bool isInitialized = false;
	private bool isCreatingSession;

	private IHostSession currentSession;
	private void Awake()
	{
        privateHostUI.Initialize(CreateSessionAsHost);
		InitializeAsync();
	}

	private async void InitializeAsync()
	{
		try
		{
			await UnityServices.InitializeAsync();
			await AuthenticationService.Instance.SignInAnonymouslyAsync();
			Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
			isInitialized = true;
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}
    
    private async Task JoinSessionAsClient()
    {
        ISession session = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode);
    }
    
	private async Task CreateSessionAsHost(SessionOptions options)
	{
		if (CanCreateSession() == false)
		{
			return;
		}

		isCreatingSession = true;

		try
		{
			currentSession = await MultiplayerService.Instance.CreateSessionAsync(options);
		}
        
		catch (SessionException exception)
		{
			Debug.LogError(
				$"Multiplayer session error.\n" +
				$"Type: {exception.GetType().FullName}\n" +
				$"Message: {exception.Message}"
			);

			Debug.LogException(exception);
		}
		catch (Exception exception)
		{
			Debug.LogError(
				$"Unexpected error while creating session.\n" +
				$"Type: {exception.GetType().FullName}\n" +
				$"Message: {exception.Message}"
			);

			Debug.LogException(exception);
		}
		finally
		{
			isCreatingSession = false;
		}
	}
    
    private bool CanCreateSession()
    {
        if (isInitialized == false)
        {
            Debug.LogWarning("Unity Services has not finished initializing.");
            return false;
        }
		
        if (isCreatingSession == true)
        {
            Debug.LogWarning("A session is already being created.");
            return false;
        }

        if (currentSession != null)
        {
            Debug.LogWarning("The player is already hosting a session.");
            return false;
        }
		
        return true;
    }
}