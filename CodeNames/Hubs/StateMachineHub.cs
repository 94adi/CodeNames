using CodeNames.Models;

namespace CodeNames.Hubs;

//TO DO:
//Instead of using global static variable, change with a nosql persistance layer
[Authorize]
public class StateMachineHub : Hub
{
    //private LiveSession _currentSession;

    private static IList<SessionUser> _queuedPlayers = new List<SessionUser>();

    private readonly IUserService _userService;

    private readonly ILiveGameSessionService _liveGameSessionService;

    private readonly IStateMachineService _stateMachineService;

    public StateMachineHub(IUserService userService,
        ILiveGameSessionService liveGameSessionService,
        IStateMachineService stateMachineService)
    {
        _userService = userService;
        _liveGameSessionService = liveGameSessionService;
        _stateMachineService = stateMachineService;
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

        }
        else if (currentSession.IdlePlayers.Where(u => u.Id == newUser.Id).FirstOrDefault() == null)
        {
            currentSession.IdlePlayers.Add(newUser);
            GameSessionDictioary.AddUserToSession(userId, connectionId, currentSession.SessionId.ToString());
        }

        //TO DO: wrap code in method call
        if (currentSession.SessionState == SessionState.PENDING)
            currentSession.SessionState = SessionState.START;

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
                NoOfCards = noCardsTargetInt
            };

            await Clients.All.SendAsync("ReceivedSpymasterClue", currentSession.Clue);

            var userIds = playerTeam.Players.Select(p => p.Id).ToList();
            userIds.Remove(player.Id);

            await Clients.Users(userIds).SendAsync("ActivateCards");

            currentSession.SessionState = _stateMachineService.NextState(currentSession.SessionState, StateTransition.NONE);
        }
    }

    public async Task PlayerSubmitGuess(string sessionId, string row, string col)
    {
        //Important: A player from the current team needs to click on EACH card, not submit multple ones at once!

        var sessionGuidId = new Guid(sessionId);    
        var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        LiveSession currentSession = GameSessionDictioary.GetSession(sessionGuidId);

        if(currentSession == null)
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

        if(player == null || playerTeam == null)
        {
            return;
        }

        bool isBlueTurn = playerTeam.Color == Color.Blue && currentSession.SessionState == SessionState.GUESS_BLUE;
        bool isRedTurn = playerTeam.Color == Color.Red && currentSession.SessionState == SessionState.GUESS_RED;

        if (!isBlueTurn && !isRedTurn)
        {
            return;
        }

        var teamIds = playerTeam.Players.Select(p => p.Id).ToArray();
        var otherTeamIds = otherTeam.Players.Select(p => p.Id).ToArray();

        int rowInt = Int32.Parse(row);
        int colInt = Int32.Parse(col);

        var guessedCard = currentSession.Grid.Cards
            .Where(c => c.Row == rowInt 
            && c.Column == colInt)
            .FirstOrDefault();

        if(guessedCard == null)
        {
            //invalid card; error
            return;
        }

        guessedCard.IsRevealed = true;
        guessedCard.ColorHex = ColorHelper.ColorToHexDict[guessedCard.Color];

        if (guessedCard.Color == Color.Black)
        {
            currentSession.SessionState = _stateMachineService.NextState(currentSession.SessionState, 
                    StateTransition.TEAM_CHOSE_BLACK_CARD);

            await Clients.All.SendAsync("DeactivateCards");
            
            await Clients.Users(teamIds).SendAsync("GameLost",
                ColorHelper.OppositeTeamsDict[playerTeam.Color].ToString(),
                ColorHelper.ColorToHexDict[ColorHelper.OppositeTeamsDict[playerTeam.Color]]);
            
            await Clients.Users(otherTeamIds).SendAsync("GameWon", playerTeam.Color.ToString(),
                ColorHelper.ColorToHexDict[playerTeam.Color]
                );
        }

        if(guessedCard.Color == playerTeam.Color)
        {
            //success!
            //TO DO: init variable
            --currentSession.NumberOfTeamActiveCards[playerTeam.Color];
            await Clients.All.SendAsync("CardGuess", rowInt, colInt, ColorHelper.ColorToHexDict[guessedCard.Color]);
            
            //TO DO: decide if team won the game here!!
            if(currentSession.NumberOfTeamActiveCards[playerTeam.Color] == 0)
            {
                await Clients.Users(otherTeamIds).SendAsync("GameLost", playerTeam.Color.ToString(), 
                    ColorHelper.ColorToHexDict[playerTeam.Color]);

                await Clients.Users(teamIds).SendAsync("GameWon",
                    ColorHelper.OppositeTeamsDict[playerTeam.Color].ToString(),
                    ColorHelper.ColorToHexDict[ColorHelper.OppositeTeamsDict[playerTeam.Color]]);

                currentSession.SessionState = _stateMachineService.NextState(currentSession.SessionState,
                    StateTransition.TEAM_GUESSED_ALL_CARDS);
            }
            //change turn to spymaster from opposite team
            //logic to decide if the user is allowed to guess another card or swith turn to opposite team spymaster
        }

        if(guessedCard.Color == Color.Neutral)
        {
            //neutral card
            currentSession.SessionState = _stateMachineService.NextState(currentSession.SessionState, StateTransition.NONE);
            await Clients.All.SendAsync("CardGuess", rowInt, colInt, ColorHelper.ColorToHexDict[guessedCard.Color]);
            //change turn to spymaster from opposite team

            string backgroundColor = ColorHelper.OppositeTeamsBackgroundColorDict[playerTeam.Color];

            await Clients.AllExcept(otherTeam.SpyMaster.ConnectionId).SendAsync("AwaitingSpymasterState", backgroundColor);

            await Clients.User(playerTeam.SpyMaster.Id).SendAsync("HideSpymasterGuessForm");

            await Clients.All.SendAsync("DeactivateCards");

            await Clients.User(otherTeam.SpyMaster.Id).SendAsync("SpyMasterMode", backgroundColor);
        }

        //if guessedCard == OtherTeamColor case
        if(guessedCard.Color == ColorHelper.OppositeTeamsDict[playerTeam.Color])
        {
            var cardsLeft = currentSession
                .NumberOfTeamActiveCards[ColorHelper.OppositeTeamsDict[playerTeam.Color]]--;

            if(cardsLeft == 0)
            {
                currentSession.SessionState = _stateMachineService.NextState(currentSession.SessionState, 
                    StateTransition.TEAM_GUESSED_ALL_OPPONENT_CARDS);

                //send signal to clients game is over
                return;
            }

            currentSession.SessionState = _stateMachineService.NextState(currentSession.SessionState, StateTransition.NONE);

            var cardColor = ColorHelper.ColorToHexDict[ColorHelper.OppositeTeamsDict[playerTeam.Color]];

            await Clients.All.SendAsync("CardGuess", rowInt, colInt, cardColor);

            string backgroundColor = ColorHelper.OppositeTeamsBackgroundColorDict[playerTeam.Color];

            await Clients.AllExcept(otherTeam.SpyMaster.ConnectionId).SendAsync("AwaitingSpymasterState", backgroundColor);

            await Clients.All.SendAsync("DeactivateCards");

            await Clients.User(playerTeam.SpyMaster.Id).SendAsync("HideSpymasterGuessForm");

            await Clients.User(otherTeam.SpyMaster.Id).SendAsync("SpyMasterMode", backgroundColor);
        }

        return;
    }

    public async Task StartGame(string sessionId)
    {
        var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionGuidId = new Guid(sessionId);

        SessionUser spyMasterBlue = null;

        LiveSession currentSession = GameSessionDictioary.GetSession(sessionGuidId);

        if (currentSession == null || currentSession.SessionState != SessionState.START)
        {
            //send signal the game couldn't be started
            return;
        }

        //TO DO: additional conditions: 3 players/team with each team having a spymaster
        //get user and verify if it's active (idle users should not see/submit the button
        if(currentSession.SessionState == SessionState.START)
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
        await Clients.AllExcept(spyMasterBlue.ConnectionId).SendAsync("AwaitingSpymasterState", backgroundColor);

        await Clients.User(spyMasterBlue.Id).SendAsync("SpyMasterMode", backgroundColor);
    }

    public override Task OnConnectedAsync()
    {
        //var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        //if (userId == null)
        //    return base.OnConnectedAsync();

        //var connectionId = Context.ConnectionId;

        //var userName = _userService.GetUserName(userId);

        //var newUser = new SessionUser { Id = userId, Name = userName, ConnectionId = connectionId };

        //_queuedPlayers.Add(newUser);

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

            if (activePlayer != null)
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
        //tag user as inactive in UI
        return base.OnDisconnectedAsync(exception);
    }
}