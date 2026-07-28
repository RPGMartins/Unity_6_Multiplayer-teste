using UnityEngine;
using Unity.Services.Core;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using Random = UnityEngine.Random;

public class SessionManager : MonoBehaviour
{
    public event Action<ISession> OnSessionCreated;
    public event Action OnSessionJoined;
    public event Action OnSessionEnded;
    public event Action OnAuthenticatedSucceed;
    public event Action OnAuthenticatedFailed;

    private bool isCreatingSession;

    private ISession currentSession;
    public ISession CurrentSession => currentSession;

    private void Awake()
    {
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            OnAuthenticatedSucceed?.Invoke();
        }
        catch (Exception e)
        {
            OnAuthenticatedFailed?.Invoke();
            Debug.LogException(e);
        }
    }

    #region Create Session

    public async Task CreateSessionAsHost(SessionOptions options)
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
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            isCreatingSession = false;
            OnSessionCreated?.Invoke(currentSession);
        }
    }

    #endregion

    #region Join Session

    public async Task JoinSessionByCode(string joinCode)
    {
        try
        {
            currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode);
            OnSessionCreated?.Invoke(currentSession);
            OnSessionJoined?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task JoinSessionByID(string joinID)
    {
        try
        {
            currentSession = await MultiplayerService.Instance.JoinSessionByIdAsync(joinID);
            OnSessionCreated?.Invoke(currentSession);
            OnSessionJoined?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task QuickJoinLobby()
    {
        QuerySessionsOptions querySessionsOptions = new QuerySessionsOptions();
        QuerySessionsResults querySessionsResults = await MultiplayerService.Instance.QuerySessionsAsync(querySessionsOptions);

        ISessionInfo session = querySessionsResults.Sessions[Random.Range(0, querySessionsResults.Sessions.Count)];
        JoinSessionByID(session.Id);
    }

    #endregion

    #region Leave Session

    public void LeaveSession()
    {
        if (currentSession.IsHost == false)
        {
            LeaveSessionAsClient();
        }
        else
        {
            //TODO migrateHost
            LeaveSessionAsClient();
        }
    }

    public async Task LeaveSessionAsClient()
    {
        await currentSession.LeaveAsync();
        currentSession = null;
        OnSessionEnded?.Invoke();
    }

    #endregion

    private bool CanCreateSession()
    {
        if (isCreatingSession == true)
        {
            Debug.LogWarning("A sessionUI is already being created.");

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
