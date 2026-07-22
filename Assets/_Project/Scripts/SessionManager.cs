using UnityEngine;
using Unity.Services.Core;
using System;
using Unity.Services.Authentication;

public class SessionManager : MonoBehaviour
{
	private void Start()
	{
		Initialize();
	}

	async void Initialize()
	{
		try
		{
			await UnityServices.InitializeAsync();
			await AuthenticationService.Instance.SignInAnonymouslyAsync();
			Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}
}