namespace CodeNames.Services.PlayerSubmitStrategy;

public class PlayerSubmitTeamCardHandler : IPlayerSubmitHandler
{
    private readonly IHubContext<StateMachineHub> _hubContext;
    private readonly IStateMachineService _stateMachineService;
    private readonly ISessionService _sessionService;

    public PlayerSubmitTeamCardHandler(IHubContext<StateMachineHub> hubContext,
        IStateMachineService stateMachineService,
        ISessionService sessionService)
    {
        _hubContext = hubContext;
        _stateMachineService = stateMachineService;
        _sessionService = sessionService;
    }

    public async Task PlayerSubmit(LiveSession session, SessionData sessionData)
    {
        --session.NumberOfTeamActiveCards[sessionData.PlayerTeam.Color];
        --session.Clue.NoOfGuessesRemaining;

        await _hubContext.Clients.All.SendAsync("CardGuess", 
            sessionData.Row, 
            sessionData.Col, 
            ColorHelper.ColorToHexDict[sessionData.GuessedCard.Color]);


        if (session.NumberOfTeamActiveCards[sessionData.PlayerTeam.Color] == 0)
        {
            await _hubContext.Clients.Users(sessionData.TeamIds).SendAsync("GameWon", 
                sessionData.PlayerTeam.Color.ToString(),
                ColorHelper.ColorToHexDict[sessionData.PlayerTeam.Color]);

            await _hubContext.Clients.Users(sessionData.OtherTeamIds).SendAsync("GameLost",
                ColorHelper.OppositeTeamsDict[sessionData.PlayerTeam.Color].ToString(),
                ColorHelper.ColorToHexDict[ColorHelper.OppositeTeamsDict[sessionData.PlayerTeam.Color]]);

            await _hubContext.Clients.All.SendAsync("OpenGameOverModal",
                sessionData.PlayerTeam.Color.ToString());

            session.SessionState = _stateMachineService.NextState(session.SessionState,
                StateTransition.TEAM_GUESSED_ALL_CARDS);

            _sessionService.EndSession(session);
            return;
        }

        if (session.Clue.NoOfGuessesRemaining == 0)
        {
            session.SessionState = _stateMachineService.NextState(session.SessionState,
                    StateTransition.TEAM_RAN_OUT_OF_GUESSES);

            string backgroundColor = ColorHelper.OppositeTeamsBackgroundColorDict[sessionData.PlayerTeam.Color];

            await _hubContext.Clients.AllExcept(sessionData.OtherTeam.SpyMaster.ConnectionId).SendAsync("AwaitingSpymasterState", 
                backgroundColor);

            await _hubContext.Clients.User(sessionData.PlayerTeam.SpyMaster.Id).SendAsync("HideSpymasterGuessForm");

            await _hubContext.Clients.All.SendAsync("DeactivateCards");

            await _hubContext.Clients.All.SendAsync("HideEndGuessButton");

            await _hubContext.Clients.User(sessionData.OtherTeam.SpyMaster.Id).SendAsync("SpyMasterMode", backgroundColor);

            return;
        }
    }
}
