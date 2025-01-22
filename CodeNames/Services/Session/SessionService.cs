namespace CodeNames.Services.Session;

public class SessionService : ISessionService
{
    private readonly ILiveGameSessionService _liveGameSessionService;
    private readonly IGameRoomService _gameRoomService;
    private readonly IHubContext<StateMachineHub> _hubContext;
    private readonly IStateMachineService _stateMachineService;

    public SessionService(ILiveGameSessionService liveGameSessionService, 
        IGameRoomService gameRoomService,
        IHubContext<StateMachineHub> hubContext,
        IStateMachineService stateMachineService)
    {
        _liveGameSessionService = liveGameSessionService;
        _gameRoomService = gameRoomService;
        _hubContext = hubContext;
        _stateMachineService = stateMachineService;
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
        var liveSession = _liveGameSessionService.GetByGameRoom(session.GameRoom.Id);

        _gameRoomService.InvalidateInvitationCode(session.GameRoom.Id, session.GameRoom.InvitationCode);

        _liveGameSessionService.Remove(liveSession);

        GameSessionDictioary.RemoveSession(session);

        GameSessionDictioary.RemoveUsersFromSession(session.SessionId.ToString());
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

    public async Task PlayerEndGuess(LiveSession session, Team playerTeam, Team otherTeam)
    {
        session.SessionState = _stateMachineService.NextState(session.SessionState, StateTransition.NONE);

        string backgroundColor = ColorHelper.OppositeTeamsBackgroundColorDict[playerTeam.Color];

        await _hubContext.Clients.AllExcept(otherTeam.SpyMaster.ConnectionId)
                                 .SendAsync("AwaitingSpymasterState", backgroundColor);

        await _hubContext.Clients.User(playerTeam.SpyMaster.Id)
                                 .SendAsync("HideSpymasterGuessForm");

        await _hubContext.Clients.All.SendAsync("DeactivateCards");

        await _hubContext.Clients.All.SendAsync("HideEndGuessButton");

        await _hubContext.Clients.User(otherTeam.SpyMaster.Id)
                                 .SendAsync("SpyMasterMode", backgroundColor);
    }

    public async Task StartGame(LiveSession currentSession)
    {
        if (currentSession == null || currentSession.SessionState != SessionState.START)
        {
            //send signal the game couldn't be started
            return;
        }

        SessionUser spyMasterBlue = null;

        //TO DO: additional conditions: 3 players/team with each team having a spymaster
        //get user and verify if it's active (idle users should not see/submit the button
        if (currentSession.SessionState == SessionState.START)
        {
            currentSession.SessionState = _stateMachineService.NextState(currentSession.SessionState, StateTransition.GAME_START);
            spyMasterBlue = currentSession.Teams.Where(t => t.Color == Color.Blue).FirstOrDefault()?.SpyMaster;
        }

        if (spyMasterBlue == null)
        {
            return;
        }
        //change UI so that only blue spymaster can give guess
        //change for ALL except for Blue Spymaster
        string backgroundColor = ColorHelper.ColorToHexDict[Color.BackgroundBlue];
        await _hubContext.Clients.AllExcept(spyMasterBlue.ConnectionId).SendAsync("AwaitingSpymasterState", 
            backgroundColor);

        await _hubContext.Clients.User(spyMasterBlue.Id).SendAsync("SpyMasterMode", backgroundColor);
    }

    public async Task TransformReturningUserToSpymaster(string sessionId, SessionUser player, string userId)
    {
        if ((player == null) || (!player.IsSpymaster))
        {
            return;
        }

        var sessionGuidId = new Guid(sessionId);

        LiveSession currentSession = GameSessionDictioary.GetSession(sessionGuidId);

        if (currentSession == null || currentSession.SessionState != SessionState.START)
        {
            //send signal to user couldn't be added
            return;
        }

        var team = currentSession.Teams.Where(t => t.Color == player.TeamColor).FirstOrDefault();

        if (team == null || (team.SpyMaster.Id != player.Id))
        {
            return;
        }

        var spymasterSecret = currentSession.Grid.Cards.Where(c => c.Color != Color.Neutral)
                .Select(c => new RevealedCard
                {
                    CardId = c.CardId,
                    Color = c.Color.ToString(),
                    Content = c.Content
                }).ToArray().ToJson();

        //the spymaster can see the color of all cards + enter data in the clue form
        await _hubContext.Clients.User(userId).SendAsync("ChangeViewToSpymaster", 
            spymasterSecret, 
            player.TeamColor.ToString());

        await _hubContext.Clients.AllExcept(player.ConnectionId).SendAsync("HideSpymasterButton", 
            player.TeamColor.ToString());
    }
}
