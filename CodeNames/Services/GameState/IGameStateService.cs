namespace CodeNames.Services.GameState;

public interface IGameStateService
{
    Task PlayerEndGuess(LiveSession currentSession, Team playerTeam, Team otherTeam);
}
