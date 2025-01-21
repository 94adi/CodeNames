namespace CodeNames.Services.PlayerSubmitStrategy;

public class PlayerSubmitOppositeTeamCardHandler : IPlayerSubmitHandler
{
    private readonly IHubContext<StateMachineHub> _hubContext;
    private readonly IStateMachineService _stateMachineService;
    private readonly ISessionService _sessionService;

    public PlayerSubmitOppositeTeamCardHandler(IHubContext<StateMachineHub> hubContext,
    IStateMachineService stateMachineService,
    ISessionService sessionService)
    {
        _hubContext = hubContext;
        _stateMachineService = stateMachineService;
        _sessionService = sessionService;
    }

    public async Task PlayerSubmit(LiveSession session, SessionData sessionData)
    {
        var cardsLeft = session
               .NumberOfTeamActiveCards[ColorHelper.OppositeTeamsDict[sessionData.PlayerTeam.Color]]--;

        if (cardsLeft == 0)
        {
            session.SessionState = _stateMachineService.NextState(session.SessionState,
                StateTransition.TEAM_GUESSED_ALL_OPPONENT_CARDS);

            await _hubContext.Clients.Users(sessionData.TeamIds).SendAsync("GameLost",
                sessionData.PlayerTeam.Color.ToString(),
                ColorHelper.ColorToHexDict[sessionData.PlayerTeam.Color]);

            await _hubContext.Clients.Users(sessionData.OtherTeamIds).SendAsync("GameWon",
                ColorHelper.OppositeTeamsDict[sessionData.PlayerTeam.Color].ToString(),
                ColorHelper.ColorToHexDict[ColorHelper.OppositeTeamsDict[sessionData.PlayerTeam.Color]]);

            await _hubContext.Clients.All.SendAsync("OpenGameOverModal",
                ColorHelper.OppositeTeamsDict[sessionData.PlayerTeam.Color].ToString());

            _sessionService.EndSession(session);

            return;
        }

        session.SessionState = _stateMachineService.NextState(session.SessionState, StateTransition.NONE);

        var cardColor = ColorHelper.ColorToHexDict[ColorHelper.OppositeTeamsDict[sessionData.PlayerTeam.Color]];
        await _hubContext.Clients.All.SendAsync("CardGuess", sessionData.Row, sessionData.Col, cardColor);

        string backgroundColor = ColorHelper.OppositeTeamsBackgroundColorDict[sessionData.PlayerTeam.Color];

        await _hubContext.Clients.AllExcept(sessionData.OtherTeam.SpyMaster.ConnectionId).SendAsync("AwaitingSpymasterState", 
            backgroundColor);

        await _hubContext.Clients.All.SendAsync("DeactivateCards");

        await _hubContext.Clients.User(sessionData.PlayerTeam.SpyMaster.Id).SendAsync("HideSpymasterGuessForm");

        await _hubContext.Clients.User(sessionData.OtherTeam.SpyMaster.Id).SendAsync("SpyMasterMode", backgroundColor);
    }
}
