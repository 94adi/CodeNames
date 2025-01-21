namespace CodeNames.Services.Session;

public interface ISessionService
{
    SessionData ExtractSessionData(LiveSession session, string userId, string row, string col);

    void EndSession(LiveSession session);

    PlayerCardSubmit CalculatePlayerSubmission(Card card, SessionData sessionData);

    Task PlayerEndGuess(LiveSession session, Team playerTeam, Team otherTeam);

    Task StartGame(LiveSession currentSession);

    Task TransformReturningUserToSpymaster(string sessionId, SessionUser player, string userId);
}
