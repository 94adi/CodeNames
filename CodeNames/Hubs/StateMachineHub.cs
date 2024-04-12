using CodeNames.Core.Services.LiveGameSessionService;
using CodeNames.Core.Services.UserService;
using CodeNames.Models;
using Microsoft.AspNetCore.SignalR;
using NuGet.Protocol;
using System.Security.Claims;

namespace CodeNames.Hubs
{
    //TO DO:
    //Instead of using global static variable, change with a nosql persistance layer
    public class StateMachineHub : Hub
    {
        private LiveSession _currentSession;

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

            _currentSession = GameSessionDictioary.GetSession(sessionGuidId);

            if (_currentSession == null)
            {
                _currentSession = new();
                _currentSession.SessionState = SessionState.Failed;
                await Clients.All.SendAsync("InvalidSession");
                return;
            }

            //register pending users from OnConnect

            if (_queuedPlayers != null && _queuedPlayers.Count > 0)
            {
                foreach (var item in _queuedPlayers)
                {
                    if (_currentSession.IdlePlayers.Where(u => u.Id.Equals(item.Id)).FirstOrDefault() == null)
                    {
                        _currentSession.IdlePlayers.Add(item);
                    }
                }
                _queuedPlayers.Clear();

            }

            GameSessionDictioary.AddUserToSession(userId, connectionId, _currentSession.SessionId.ToString());

            if(_currentSession.SessionState == SessionState.Pending)
                _currentSession.SessionState = SessionState.Start;

            var idlePlayersJson = _currentSession.IdlePlayers.ToJson();

            await Clients.User(Context.ConnectionId).SendAsync("GameSessionStart");

            await Clients.All.SendAsync("RefreshIdlePlayersList", idlePlayersJson);
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return base.OnConnectedAsync();

            var connectionId = Context.ConnectionId;

            var userName = _userService.GetUserName(userId);

            var newUser = new SessionUser { Id = userId, Name = userName, ConnectionId = Context.ConnectionId };

            //it's always gonna be null; gotta add user to session first!
            var currentSession = GameSessionDictioary.GetUserSession(userId, connectionId);

            if (currentSession == null)
            {       
                _queuedPlayers.Add(newUser);
                return base.OnConnectedAsync();
            }

            //if(_queuedPlayers != null && _queuedPlayers.Count > 0) 
            //{
            //    foreach(var item in _queuedPlayers)
            //    {
            //        var isUserRegistered = currentSession.IdlePlayers.Where(u => u.Id.Equals(item.Id)).FirstOrDefault() == null ? false : true;

            //        if (!isUserRegistered)
            //        {
            //            currentSession.IdlePlayers.Add(item);
            //        }
            //    }
            //    _queuedPlayers.Clear();

            //}

            //if (currentSession.IdlePlayers.Where(u => u.Id.Equals(userId)).FirstOrDefault() == null)
            //{
            //    currentSession.IdlePlayers.Add(newUser);
            //}
            //else
            //{
            //    newUser.ConnectionId = Context.ConnectionId;
            //}

            //Clients.All.SendAsync("RefreshIdlePlayersList", currentSession.IdlePlayers.ToJson());

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

            var queuedUser = _queuedPlayers.Where(u => u.Id == userId).FirstOrDefault();

            if ((_queuedPlayers.Count > 0) && (queuedUser != null))
            {
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
                            }
                        }
                    }

                }

                if(currentSession != null && currentSession.IdlePlayers != null && currentSession.IdlePlayers.Count == 0 )
                {
                    var liveGameSession = _liveGameSessionService.GetByGameRoom(currentSession.GameRoom.Id);

                    _liveGameSessionService.Remove(liveGameSession);

                    GameSessionDictioary.RemoveSession(currentSession);

                    return base.OnDisconnectedAsync(exception);
                }

                Clients.All.SendAsync("RefreshIdlePlayersList", currentSession?.IdlePlayers.ToJson());
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
