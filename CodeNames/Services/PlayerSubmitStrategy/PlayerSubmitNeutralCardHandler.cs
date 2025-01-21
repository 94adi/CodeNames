namespace CodeNames.Services.PlayerSubmitStrategy;

public class PlayerSubmitNeutralCardHandler : IPlayerSubmitHandler
{
    private readonly IHubContext<StateMachineHub> _hubContext;
    private readonly IGameStateService _gameStateService;

    public PlayerSubmitNeutralCardHandler(IHubContext<StateMachineHub> hubContext,
        IGameStateService gameStateService)
    {
        _hubContext = hubContext;
        _gameStateService = gameStateService;
    }

    public async Task PlayerSubmit(LiveSession session, SessionData sessionData)
    {
        await _gameStateService.PlayerEndGuess(session, sessionData.PlayerTeam, sessionData.OtherTeam);

        await _hubContext.Clients.All.SendAsync("CardGuess", 
            sessionData.Row, 
            sessionData.Col, 
            ColorHelper.ColorToHexDict[sessionData.GuessedCard.Color]);
    }
}
