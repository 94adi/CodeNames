﻿using CodeNames.Services.PlayerSubmitStrategy.Factory;
using CodeNames.Services.Session;

namespace CodeNames.Hubs;

[Authorize]
public class StateMachineHub : Hub
{
    private readonly IUserService _userService;
    private readonly ILiveGameSessionService _liveGameSessionService;
    private readonly IGameRoomService _gameRoomService;
    private readonly IStateMachineService _stateMachineService;
    private readonly ISessionService _sessionService;
    private readonly IPlayerSubmitFactory _playerSubmitFactory;

    public StateMachineHub(IUserService userService,
        ILiveGameSessionService liveGameSessionService,
        IStateMachineService stateMachineService,
        IGameRoomService gameRoomService,
        IPlayerSubmitFactory playerSubmitFactory)
    {
        _userService = userService;
        _liveGameSessionService = liveGameSessionService;
        _stateMachineService = stateMachineService;
        _gameRoomService = gameRoomService;
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
                await this._TransformReturningUserToSpymaster(sessionId, player);
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
            await _StartGame(currentSession);
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

        await _PlayerEndGuess(currentSession, playerTeam, otherTeam);
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

        if (guessedCard.Color == sessionData.PlayerTeam.Color)
        {
            --currentSession.NumberOfTeamActiveCards[sessionData.PlayerTeam.Color];
            --currentSession.Clue.NoOfGuessesRemaining;
            await Clients.All.SendAsync("CardGuess", sessionData.Row, sessionData.Col, ColorHelper.ColorToHexDict[guessedCard.Color]);
            
            
            if(currentSession.NumberOfTeamActiveCards[playerTeam.Color] == 0)
            {
                await Clients.Users(teamIds).SendAsync("GameWon", playerTeam.Color.ToString(), 
                    ColorHelper.ColorToHexDict[playerTeam.Color]);

                await Clients.Users(otherTeamIds).SendAsync("GameLost",
                    ColorHelper.OppositeTeamsDict[playerTeam.Color].ToString(),
                    ColorHelper.ColorToHexDict[ColorHelper.OppositeTeamsDict[playerTeam.Color]]);

                await Clients.All.SendAsync("OpenGameOverModal", 
                    playerTeam.Color.ToString());

                currentSession.SessionState = _stateMachineService.NextState(currentSession.SessionState,
                    StateTransition.TEAM_GUESSED_ALL_CARDS);

                await _EndSession(currentSession);
                return;
            }
            //change turn to spymaster from opposite team
            //logic to decide if the user is allowed to guess another card or swith turn to opposite team spymaster
            if (currentSession.Clue.NoOfGuessesRemaining == 0)
            {
                currentSession.SessionState = _stateMachineService.NextState(currentSession.SessionState,
                        StateTransition.TEAM_RAN_OUT_OF_GUESSES);

                string backgroundColor = ColorHelper.OppositeTeamsBackgroundColorDict[playerTeam.Color];

                await Clients.AllExcept(otherTeam.SpyMaster.ConnectionId).SendAsync("AwaitingSpymasterState", backgroundColor);

                await Clients.User(playerTeam.SpyMaster.Id).SendAsync("HideSpymasterGuessForm");

                await Clients.All.SendAsync("DeactivateCards");

                await Clients.All.SendAsync("HideEndGuessButton");

                await Clients.User(otherTeam.SpyMaster.Id).SendAsync("SpyMasterMode", backgroundColor);

                return;
            }

        }

        if(guessedCard.Color == Color.Neutral)
        {
            await _PlayerEndGuess(currentSession, playerTeam, otherTeam);

            await Clients.All.SendAsync("CardGuess", rowInt, colInt, ColorHelper.ColorToHexDict[guessedCard.Color]);        
        }

        
        if(guessedCard.Color == ColorHelper.OppositeTeamsDict[playerTeam.Color])
        {
            var cardsLeft = currentSession
                .NumberOfTeamActiveCards[ColorHelper.OppositeTeamsDict[playerTeam.Color]]--;

            if(cardsLeft == 0)
            {
                currentSession.SessionState = _stateMachineService.NextState(currentSession.SessionState, 
                    StateTransition.TEAM_GUESSED_ALL_OPPONENT_CARDS);

                await Clients.Users(teamIds).SendAsync("GameLost", 
                    playerTeam.Color.ToString(),
                    ColorHelper.ColorToHexDict[playerTeam.Color]);

                await Clients.Users(otherTeamIds).SendAsync("GameWon", 
                    ColorHelper.OppositeTeamsDict[playerTeam.Color].ToString(),
                    ColorHelper.ColorToHexDict[ColorHelper.OppositeTeamsDict[playerTeam.Color]]);

                await Clients.All.SendAsync("OpenGameOverModal",
                    ColorHelper.OppositeTeamsDict[playerTeam.Color].ToString());

                await _EndSession(currentSession);

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

    private async Task _StartGame(LiveSession currentSession)
    {
        if (currentSession == null || currentSession.SessionState != SessionState.START)
        {
            //send signal the game couldn't be started
            return;
        }

        var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
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
        await Clients.AllExcept(spyMasterBlue.ConnectionId).SendAsync("AwaitingSpymasterState", backgroundColor);

        await Clients.User(spyMasterBlue.Id).SendAsync("SpyMasterMode", backgroundColor);
    }

    private async Task _TransformReturningUserToSpymaster(string sessionId, SessionUser player)
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

        var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        //the spymaster can see the color of all cards + enter data in the clue form
        await Clients.User(userId).SendAsync("ChangeViewToSpymaster", spymasterSecret, player.TeamColor.ToString());

        await Clients.AllExcept(player.ConnectionId).SendAsync("HideSpymasterButton", player.TeamColor.ToString());
    }

    private async Task _EndSession(LiveSession session)
    {
        //call service here
        
    }

    private async Task _PlayerEndGuess(
        LiveSession currentSession,
        Team playerTeam,
        Team otherTeam)
    {
        //TO DO: verify if you're in the right SessionState to end the guess
        currentSession.SessionState = _stateMachineService.NextState(currentSession.SessionState, StateTransition.NONE);

        string backgroundColor = ColorHelper.OppositeTeamsBackgroundColorDict[playerTeam.Color];

        await Clients.AllExcept(otherTeam.SpyMaster.ConnectionId).SendAsync("AwaitingSpymasterState", backgroundColor);

        await Clients.User(playerTeam.SpyMaster.Id).SendAsync("HideSpymasterGuessForm");

        await Clients.All.SendAsync("DeactivateCards");

        await Clients.All.SendAsync("HideEndGuessButton");

        await Clients.User(otherTeam.SpyMaster.Id).SendAsync("SpyMasterMode", backgroundColor);
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

                _EndSession(currentSession).GetAwaiter().GetResult();
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