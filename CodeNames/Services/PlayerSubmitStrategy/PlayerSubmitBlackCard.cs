using CodeNames.Services.Session;

namespace CodeNames.Services.PlayerSubmitStrategy;

public class PlayerSubmitBlackCard : IPlayerSubmitStrategy
{
    private readonly IHubContext<StateMachineHub> _hubContext;
    private readonly IStateMachineService _stateMachineService;
    private readonly ISessionService _sessionService;

    public PlayerSubmitBlackCard(IHubContext<StateMachineHub> hubContext,
        IStateMachineService stateMachineService,
        ISessionService sessionService)
    {
        _hubContext = hubContext;
        _stateMachineService = stateMachineService;
        _sessionService = sessionService;
    }
    public async Task PlayerSubmit(LiveSession session, SessionData sessionData)
    {
        session.SessionState = _stateMachineService.NextState(session.SessionState,
        StateTransition.TEAM_CHOSE_BLACK_CARD);

        await _hubContext.Clients.All.SendAsync("DeactivateCards");

        _sessionService.EndSession(session);

        await _hubContext.Clients.All.SendAsync("EndSession");

        await _hubContext.Clients.Users(sessionData.TeamIds).SendAsync("GameLost",
            sessionData.PlayerTeam.Color.ToString(),
            ColorHelper.ColorToHexDict[sessionData.PlayerTeam.Color]);

        await _hubContext.Clients.Users(sessionData.OtherTeamIds).SendAsync("GameWon",
            ColorHelper.OppositeTeamsDict[sessionData.PlayerTeam.Color].ToString(),
            ColorHelper.ColorToHexDict[ColorHelper.OppositeTeamsDict[sessionData.PlayerTeam.Color]]);

        await _hubContext.Clients.All.SendAsync("OpenGameOverModal",
            ColorHelper.OppositeTeamsDict[sessionData.PlayerTeam.Color].ToString());
    }
}
