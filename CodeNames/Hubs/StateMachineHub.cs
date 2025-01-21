namespace CodeNames.Hubs;

[Authorize]
public class StateMachineHub : Hub
{
    private readonly IUserService _userService;
    private readonly IStateMachineService _stateMachineService;
    private readonly ISessionService _sessionService;
    private readonly IPlayerSubmitFactory _playerSubmitFactory;

    public StateMachineHub(IUserService userService,
        IStateMachineService stateMachineService,
        IPlayerSubmitFactory playerSubmitFactory)
    {
        _userService = userService;
        _stateMachineService = stateMachineService;
        _playerSubmitFactory = playerSubmitFactory;
    }

    public async Task ReceiveSessionId(string sessionId)
    {
        var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null) 
        {
            await Clients.All.SendAsync("InvalidSession");
            return; 
        }

        var connectionId = Context.ConnectionId;
        var sessionGuidId = new Guid(sessionId);

        LiveSession currentSession = GameSessionDictioary.GetSession(sessionGuidId);

        if (currentSession == null)
        {
            currentSession = new();
            currentSession.SessionState = SessionState.FAILURE;
            await Clients.All.SendAsync("InvalidSession");
            return;
        }

        if (currentSession.SessionState == SessionState.PENDING)
            currentSession.SessionState = SessionState.START;

        var userName = _userService.GetUserName(userId);

        var newUser = new SessionUser { 
            Id = userId, 
            Name = userName, 
            ConnectionId = connectionId,
            UserStatus = UserStatus.Active,
            TeamColor = Color.Neutral
        };

        var player = currentSession.PlayersList.Where(p => p.Id == newUser.Id).FirstOrDefault();
        bool isPlayerAlreadyJoined = player != null ? true : false;

        if (isPlayerAlreadyJoined)
        {
            //TO DO CHANGES AFTER CONTROLLER
            player.ConnectionId = newUser.ConnectionId;

            GameSessionDictioary.AddUserToSession(player.Id, 
                player.ConnectionId, 
                currentSession.SessionId.ToString());

            player.UserStatus = UserStatus.Active;

            if((!player.IsSpymaster) && currentSession.SessionState == SessionState.START)
            {
                await Clients.User(userId).SendAsync("ChangeJoinButtonToSpymaster", player.TeamColor.ToString());
                //await Clients.User(userId).SendAsync("AddSpymasterHandlerToBtn", player.TeamColor.ToString());
            }
            else if (player.IsSpymaster && currentSession.SessionState == SessionState.START)
            {
                await _sessionService.TransformReturningUserToSpymaster(sessionId, player, userId);
            }

        }
        else if (currentSession.IdlePlayers.Where(u => u.Id == newUser.Id).FirstOrDefault() == null)
        {
            currentSession.IdlePlayers.Add(newUser);
            GameSessionDictioary.AddUserToSession(userId, connectionId, currentSession.SessionId.ToString());
        }

        var idlePlayersJson = currentSession.IdlePlayers.ToJson();

        await Clients.User(Context.ConnectionId).SendAsync("GameSessionStart");

        await Clients.All.SendAsync("RefreshIdlePlayersList", idlePlayersJson);

        foreach(var team in currentSession.Teams)
        {
            await Clients.All.SendAsync("RefreshTeamPlayer", team.Color.ToString(), team.Players.ToJson());
        }  
    }

    public async Task UserJoinedTeam(string sessionId, string teamColor)
    {
        var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var connectionId = Context.ConnectionId;
        var sessionGuidId = new Guid(sessionId);
        var userName = _userService.GetUserName(userId);

        var player = new SessionUser
        {
            Id = userId,
            Name = userName,
            ConnectionId = connectionId
        };

        LiveSession currentSession = GameSessionDictioary.GetSession(sessionGuidId);

        if(currentSession == null || currentSession.SessionState != SessionState.START)
        {
            //send signal to user couldn't be added
            return;
        }

        bool userAlreadyJoined = false;

        foreach(var team in currentSession.Teams)
        {
            if (team.Players.Contains(player))
            {
                userAlreadyJoined = true;
                break;
            }
        }

        if (userAlreadyJoined) return;

        Enum.TryParse(teamColor, out Color enumTeamColor);

        var selectedTeam = currentSession.Teams.Where(t => t.Color == enumTeamColor).FirstOrDefault();
        if(selectedTeam != null)
        {
            player.TeamColor = enumTeamColor;
            selectedTeam?.Players.Add(player);
            currentSession.PlayersList.Add(player);
            var playerToRemove = currentSession.IdlePlayers.Where(p => p.Id == player.Id).FirstOrDefault();
            currentSession.IdlePlayers.Remove(playerToRemove);

            await Clients.All.SendAsync("RefreshTeamPlayer", teamColor, selectedTeam?.Players.ToJson());

            await Clients.All.SendAsync("RefreshIdlePlayersList", currentSession.IdlePlayers.ToJson());

            await Clients.User(userId).SendAsync("ChangeJoinButtonToSpymaster", teamColor);

            return;
        }

        //something went wrong :(

    }

    public async Task TransformUserToSpymaster(string sessionId, string teamColor)
    {
        var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionGuidId = new Guid(sessionId);

        LiveSession currentSession = GameSessionDictioary.GetSession(sessionGuidId);

        if (currentSession == null || currentSession.SessionState != SessionState.START)
        {
            //send signal to user couldn't be added
            return;
        }

        Enum.TryParse(teamColor, out Color enumTeamColor);

        var team = currentSession.Teams.Where(t => t.Color == enumTeamColor).FirstOrDefault();

        if (team == null)
        {
            return;
        }

        bool hasSpymaster = team.Players.Where(p => p.IsSpymaster == true).FirstOrDefault() != null ? true : false;
        if (hasSpymaster) return;

        var player = team.Players.Where(p => p.Id == userId).FirstOrDefault();

        if(player == null)
        {
            return;
        }

        player.IsSpymaster = true;

        team.SpyMaster = player;

        var spymasterSecret = currentSession.Grid.Cards.Where(c => c.Color != Color.Neutral)
                .Select(c => new RevealedCard
                {
                    CardId = c.CardId,
                    //TO DO: use the color dictionary instead for hex values
                    //Add text color value as well
                    Color = c.Color.ToString(),
                    Content = c.Content
                }).ToArray().ToJson();

        //the spymaster can see the color of all cards + enter data in the clue form
        await Clients.User(userId).SendAsync("ChangeViewToSpymaster", spymasterSecret, teamColor);

        await Clients.AllExcept(player.ConnectionId).SendAsync("HideSpymasterButton", teamColor);

        //verify if game is ready to start
        //CONDITION: both teams need at least 2 members and a spymaster
        bool isGameReady = true;
        foreach(var item in currentSession.Teams)
        {
            bool teamReadyCondition = ((item.Players.Count() > 1) && (item.SpyMaster != null));
            if(!teamReadyCondition)
            {
                isGameReady = false;
                break;
            }
        }

        if (isGameReady) 
        {
            //start game
            await _sessionService.StartGame(currentSession);
        }

    }

    public async Task SpymasterSubmitGuess(string sessionId, string clue, string noCardsTarget)
    {
        var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionGuidId = new Guid(sessionId);

        LiveSession currentSession = GameSessionDictioary.GetSession(sessionGuidId);

        if (currentSession == null || 
            (currentSession.SessionState != SessionState.SPYMASTER_RED && 
            currentSession.SessionState != SessionState.SPYMASTER_BLUE))
        {
            //send signal to user couldn't be added
            return;
        }

        //find player by Id 
        SessionUser player = null;
        Team playerTeam = null;

        //TO DO: better way to get the spymaster player's team
        foreach (var team in currentSession.Teams)
        {
            player = team.Players.Where(p => p.Id == userId).FirstOrDefault();
            if (player != null)
            {
                playerTeam = team;
                break;
            }
        }

        if (player == null || !player.IsSpymaster)
        {
            return;
        }

        var teamColor = playerTeam.Color;

        bool isBlueTurn = currentSession.SessionState == SessionState.SPYMASTER_BLUE && teamColor == Color.Blue;
        bool isRedTurn = currentSession.SessionState == SessionState.SPYMASTER_RED && teamColor == Color.Red;

        if (isBlueTurn || isRedTurn)
        {
            int noCardsTargetInt = Int32.Parse(noCardsTarget);

            currentSession.Clue = new Clue
            {
                Word = clue,
                NoOfCards = noCardsTargetInt,
                NoOfGuessesRemaining = (noCardsTargetInt + 1)
            };

            await Clients.All.SendAsync("ReceivedSpymasterClue", currentSession.Clue);

            var userIds = playerTeam.Players.Select(p => p.Id).ToList();
            userIds.Remove(player.Id);

            await Clients.Users(userIds).SendAsync("ActivateCards");
            await Clients.Users(userIds).SendAsync("ShowEndGuessButton");            

            currentSession.SessionState = _stateMachineService.NextState(currentSession.SessionState, StateTransition.NONE);
        }
    }

    public async Task PlayerSubmitEndGuess(string sessionId)
    {
        var sessionGuidId = new Guid(sessionId);

        LiveSession currentSession = GameSessionDictioary.GetSession(sessionGuidId);

        var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentSession == null)
        {
            return;
        }

        SessionUser player = null;
        Team playerTeam = null;
        Team otherTeam = null;

        foreach (var team in currentSession.Teams)
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

        if (player == null || playerTeam == null)
        {
            return;
        }

        await _sessionService.PlayerEndGuess(currentSession, playerTeam, otherTeam);
    }

    public async Task PlayerSubmitGuess(string sessionId, string row, string col)
    {
        var sessionGuidId = new Guid(sessionId);    
        var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        LiveSession currentSession = GameSessionDictioary.GetSession(sessionGuidId);

        var sessionData = _sessionService.ExtractSessionData(currentSession, userId, row, col);

        var guessedCard = sessionData.GuessedCard;

        bool isBlueTurn = sessionData.PlayerTeam.Color == Color.Blue && currentSession.SessionState == SessionState.GUESS_BLUE;
        bool isRedTurn = sessionData.PlayerTeam.Color == Color.Red && currentSession.SessionState == SessionState.GUESS_RED;

        bool earlyReturnCondition = ((!isBlueTurn && !isRedTurn) || (guessedCard == null));

        if (earlyReturnCondition)
        {
            return;
        }

        guessedCard.IsRevealed = true;
        guessedCard.ColorHex = ColorHelper.ColorToHexDict[guessedCard.Color];

        var submissionType = _sessionService.CalculatePlayerSubmission(guessedCard, sessionData);

        var submissionHandler = _playerSubmitFactory.Create(submissionType);

        await submissionHandler.PlayerSubmit(currentSession, sessionData);

        return;
    }

    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {

        var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return base.OnDisconnectedAsync(exception);
        }

        var connectionId = Context.ConnectionId;

        var currentSession = GameSessionDictioary.GetUserSession(userId, connectionId);

        if (currentSession != null)
        {
            var idlePlayer = currentSession.IdlePlayers.Where(u => u.Id == userId).FirstOrDefault();
            var activePlayer = currentSession.PlayersList.Where(u => u.Id == userId).FirstOrDefault();

            GameSessionDictioary.RemoveUserFromSesion(userId, 
                connectionId, 
                currentSession.SessionId.ToString());

            if (idlePlayer != null)
            {
                currentSession.IdlePlayers.Remove(idlePlayer);
                Clients.All.SendAsync("RefreshIdlePlayersList", currentSession?.IdlePlayers.ToJson());
            }

            else if(activePlayer != null && 
                activePlayer.IsSpymaster && 
                GameSessionHelper.IsGameOngoing(currentSession))
            {
                currentSession.SessionState = SessionState.FAILURE;
                //send failure signal
                Clients.All.SendAsync("GameFailureSignal").GetAwaiter().GetResult();

                Clients.All.SendAsync("OpenSpymasterModal", activePlayer.TeamColor.ToString())
                           .GetAwaiter()
                           .GetResult();

                _sessionService.EndSession(currentSession);
            }

            else if (activePlayer != null)
            {
                activePlayer.UserStatus = UserStatus.Inactive;
                //currentSession.PlayersList.Remove(activePlayer);

                foreach (var team in currentSession.Teams)
                {
                    if (team.Players != null && team.Players.Count > 0)
                    {
                        Clients.All.SendAsync("RefreshTeamPlayer", 
                            team.Color.ToString(), 
                            team.Players.ToJson());
                    }
                }
                
            }
        }
        return base.OnDisconnectedAsync(exception);
    }
}