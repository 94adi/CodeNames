namespace CodeNames.Services.Session;

public class SessionService : ISessionService
{
    private readonly ILiveGameSessionService _liveGameSessionService;
    private readonly IGameRoomService _gameRoomService;

    public SessionService(ILiveGameSessionService liveGameSessionService, 
        IGameRoomService gameRoomService)
    {
        _liveGameSessionService = liveGameSessionService;
        _gameRoomService = gameRoomService;
    }

    public PlayerCardSubmit CalculatePlayerSubmission(Card card, SessionData sessionData)
    {
        Color playerTeamColor = sessionData.PlayerTeam.Color;
        Color otherTeamColor = ColorHelper.OppositeTeamsDict[playerTeamColor];
        Color cardColor = card.Color;

        PlayerCardSubmit result = cardColor switch
        {
            Color.Black => PlayerCardSubmit.Black,
            _ when cardColor == playerTeamColor => PlayerCardSubmit.Team,
            _ when cardColor == otherTeamColor => PlayerCardSubmit.OppositeTeam,
            Color.Neutral => PlayerCardSubmit.Neutral,
            _ => throw new Exception("Invalid card color")
        };

        return result;
    }

    public void EndSession(LiveSession session)
    {
        GameSessionDictioary.RemoveSession(session);

        GameSessionDictioary.RemoveUsersFromSession(session.SessionId.ToString());

        var liveSession = _liveGameSessionService.GetByGameRoom(session.GameRoom.Id);

        _gameRoomService.InvalidateInvitationCode(session.GameRoom.Id, session.GameRoom.InvitationCode);

        _liveGameSessionService.Remove(liveSession);
    }

    public SessionData ExtractSessionData(LiveSession session, string userId, string row, string col)
    {
        if (session == null || string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("Invalid arguments");
        }

        SessionUser player = null;
        Team playerTeam = null;
        Team otherTeam = null;

        foreach (var team in session.Teams)
        {
            var currentPlayer = team.Players.Where(p => p.Id == userId).FirstOrDefault();
            if (currentPlayer != null)
            {
                player = currentPlayer;
                playerTeam = team;
            }
            else
            {
                otherTeam = team;
            }
        }

        if (player == null || playerTeam == null || otherTeam == null)
        {
            throw new Exception("Could not extract data");
        }

        var teamIds = playerTeam.Players.Select(p => p.Id).ToArray();
        var otherTeamIds = otherTeam.Players.Select(p => p.Id).ToArray();

        int rowInt = int.Parse(row);
        int colInt = int.Parse(col);

        var guessedCard = session.Grid.Cards
            .Where(c => c.Row == rowInt
            && c.Column == colInt)
            .FirstOrDefault();

        if (guessedCard == null)
        {
            throw new Exception("Could not find the card");
        }

        return new SessionData(session.SessionId,
            player,
            playerTeam,
            otherTeam,
            teamIds,
            otherTeamIds,
            guessedCard,
            rowInt,
            colInt);
    }
}
