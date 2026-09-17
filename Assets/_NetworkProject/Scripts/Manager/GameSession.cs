using System.Threading.Tasks;
using Unity.Services.Multiplayer;

public class GameSession
{
    private LobbyData lobbyData;
    private ISession onlineSession;

    public LobbyData LobbyData => lobbyData;
    public ISession OnlineSession => onlineSession;

    public bool IsOnline => onlineSession != null;

    public GameSession(ISession onlineSession)
    {
        this.onlineSession = onlineSession;
    }

    public GameSession(LobbyData lobbyData, ISession onlineSession)
    {
        this.lobbyData = lobbyData;
        this.onlineSession = onlineSession;
    }

    public async Task LeaveSessionAsync()
    {
        if (onlineSession != null)
        {
            await onlineSession.LeaveAsync();
        }
    }
}
