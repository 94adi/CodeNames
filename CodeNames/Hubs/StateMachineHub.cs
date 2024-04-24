using CodeNames.Core.Services.LiveGameSessionService;
using CodeNames.Core.Services.UserService;
using CodeNames.Models;
using Microsoft.AspNetCore.SignalR;
using NuGet.Protocol;
using System.Numerics;
using System.Security.Claims;

namespace CodeNames.Hubs
{
    //TO DO:
    //Instead of using global static variable, change with a nosql persistance layer
    public class StateMachineHub : Hub
    {
        //private LiveSession _currentSession;

        private static IList<SessionUser> _queuedPlayers = new List<SessionUser>();

        private readonly IUserService _userService;

        private readonly ILiveGameSessionService _liveGameSessionService;

        public StateMachineHub(IUserService userService,
            ILiveGameSessionService liveGameSessionService)
        {
            _userService = userService;
            _liveGameSessionService = liveGameSessionService;
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
                currentSession.SessionState = SessionState.Failed;
                await Clients.All.SendAsync("InvalidSession");
                return;
            }

            //register pending users from OnConnect

            if (_queuedPlayers != null && _queuedPlayers.Count > 0)
            {
                foreach (var item in _queuedPlayers)
                {
                    if (currentSession.IdlePlayers.Where(u => u.Id.Equals(item.Id)).FirstOrDefault() == null)
                    {
                        currentSession.IdlePlayers.Add(item);
                    }
                }
                _queuedPlayers.Clear();

            }

            GameSessionDictioary.AddUserToSession(userId, connectionId, currentSession.SessionId.ToString());

            if(currentSession.SessionState == SessionState.Pending)
                currentSession.SessionState = SessionState.Start;

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

            if(currentSession == null || currentSession.SessionState != SessionState.Start)
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

            if (currentSession == null || currentSession.SessionState != SessionState.Start)
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
            await Clients.User(userId).SendAsync("ChangeViewToSpymaster", spymasterSecret);

            await Clients.AllExcept(player.ConnectionId).SendAsync("RemoveSpymasterButton", teamColor);
        }

        public async Task SpymasterSubmitGuess(string sessionId, string clue, string noCardsTarget)
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionGuidId = new Guid(sessionId);

            LiveSession currentSession = GameSessionDictioary.GetSession(sessionGuidId);

            if (currentSession == null || 
                (currentSession.SessionState == SessionState.SpymasterRed || currentSession.SessionState == SessionState.SpymasterBlue))
            {
                //send signal to user couldn't be added
                return;
            }

            //find player by Id 
            SessionUser player = null;
            Team playerTeam = null;
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

            bool isBlueTurn = currentSession.SessionState == SessionState.GuessBlue && teamColor == Color.Blue;
            bool isRedTurn = currentSession.SessionState == SessionState.GuessRed && teamColor == Color.Red;

            if (true /*isBlueTurn || isRedTurn*/)
            {
                int noCardsTargetInt = Int32.Parse(noCardsTarget);
                currentSession.Clue = new Clue
                {
                    Word = clue,
                    NoOfCards = noCardsTargetInt
                };

                //broadcast clue
                await Clients.All.SendAsync("ReceivedSpymasterClue", currentSession.Clue);
            }
        }

        public async Task StartGame(string sessionId)
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionGuidId = new Guid(sessionId);

            SessionUser spyMasterBlue = null;

            LiveSession currentSession = GameSessionDictioary.GetSession(sessionGuidId);

            if (currentSession == null || currentSession.SessionState != SessionState.Start)
            {
                //send signal the game couldn't be started
                return;
            }

            //TO DO: additional conditions: 3 players/team with each team having a spymaster
            //get user and verify if it's active (idle users should not see/submit the button
            if(currentSession.SessionState == SessionState.Start)
            {
                currentSession.SessionState = SessionState.SpymasterBlue;
                spyMasterBlue = currentSession.Teams.Where(t => t.Color == Color.Blue).FirstOrDefault()?.SpyMaster;
            }

            if (spyMasterBlue == null)
            {
                return;
            }
            //change UI so that only blue spymaster can give guess
            //change for ALL except for Blue Spymaster
            string backgroundColor = StaticDetails.ColorToHexDict[Color.BackgrounBlue];
            await Clients.AllExcept(spyMasterBlue.ConnectionId).SendAsync("AwaitingSpymasterState", backgroundColor);

            await Clients.User(spyMasterBlue.Id).SendAsync("SpyMasterMode", backgroundColor);
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return base.OnConnectedAsync();

            var connectionId = Context.ConnectionId;

            var userName = _userService.GetUserName(userId);

            var newUser = new SessionUser { Id = userId, Name = userName, ConnectionId = connectionId };
    
            _queuedPlayers.Add(newUser);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {

            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
            {
                return base.OnDisconnectedAsync(exception);
            }

            var connectionId = Context.ConnectionId;

            

            if (_queuedPlayers != null && _queuedPlayers.Count > 0)
            {
                var queuedUser = _queuedPlayers.Where(u => u.Id == userId).FirstOrDefault();
                if(queuedUser != null) 
                    _queuedPlayers.Remove(queuedUser);
            }

            var currentSession = GameSessionDictioary.GetUserSession(userId, connectionId);

            if(currentSession != null)
            {
                var idlePlayer = currentSession.IdlePlayers.Where(u => u.Id == userId).FirstOrDefault();
                var activePlayer = currentSession.PlayersList.Where(u => u.Id == userId).FirstOrDefault();

                if (idlePlayer != null)
                {
                    currentSession.IdlePlayers.Remove(idlePlayer);
                    Clients.All.SendAsync("RefreshIdlePlayersList", currentSession?.IdlePlayers.ToJson());
                }

                if (activePlayer != null)
                {
                    currentSession.PlayersList.Remove(activePlayer);

                    foreach (var team in currentSession.Teams)
                    {
                        if (team.Players != null && team.Players.Count > 0)
                        {
                            var teamPlayer = team.Players?.Where(u => u.Id == userId).FirstOrDefault();

                            if (teamPlayer != null)
                            {
                                team.Players?.Remove(teamPlayer);
                                Clients.All.SendAsync("RefreshTeamPlayer", team.Color.ToString(), team.Players.ToJson());
                                //return base.OnDisconnectedAsync(exception);
                            }
                        }
                    }

                }

                bool isIdlePLayersListEmpty = (currentSession.IdlePlayers != null && currentSession.IdlePlayers.Count == 0);
                bool isActivePlayersListEMpty = (currentSession.PlayersList != null && currentSession.PlayersList.Count == 0);

                if(isIdlePLayersListEmpty && isActivePlayersListEMpty)
                {
                    var liveGameSession = _liveGameSessionService.GetByGameRoom(currentSession.GameRoom.Id);

                    _liveGameSessionService.Remove(liveGameSession);

                    GameSessionDictioary.RemoveSession(currentSession);

                    return base.OnDisconnectedAsync(exception);
                }

            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
