using CodeNames.Services.Session;

namespace CodeNames.Services.GameState;

public class GameStateService : IGameStateService
{
    private readonly IHubContext<StateMachineHub> _hubContext;
    private readonly IStateMachineService _stateMachineService;
    private readonly ISessionService _sessionService;

    public GameStateService(IHubContext<StateMachineHub> hubContext,
        IStateMachineService stateMachineService,
        ISessionService sessionService)
    {
        _hubContext = hubContext;
        _stateMachineService = stateMachineService;
        _sessionService = sessionService;
    }

    public async Task PlayerEndGuess(LiveSession currentSession, Team playerTeam, Team otherTeam)
    {
        currentSession.SessionState = _stateMachineService.NextState(currentSession.SessionState, StateTransition.NONE);

        string backgroundColor = ColorHelper.OppositeTeamsBackgroundColorDict[playerTeam.Color];

        await _hubContext.Clients.AllExcept(otherTeam.SpyMaster.ConnectionId).SendAsync("AwaitingSpymasterState", backgroundColor);

        await _hubContext.Clients.User(playerTeam.SpyMaster.Id).SendAsync("HideSpymasterGuessForm");

        await _hubContext.Clients.All.SendAsync("DeactivateCards");

        await _hubContext.Clients.All.SendAsync("HideEndGuessButton");

        await _hubContext.Clients.User(otherTeam.SpyMaster.Id).SendAsync("SpyMasterMode", backgroundColor);
    }
}
