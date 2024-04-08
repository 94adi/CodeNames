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

        public StateMachineHub(IUserService userService)
        {
            _userService = userService;
        }
        public async Task ReceiveSessionId(string sessionId)
        {
            var sessionGuidId = new Guid(sessionId);

            //_currentSession = GameSessionDictioary.GetSession(new Guid());
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

            _currentSession.SessionState = SessionState.Start;
            var idlePlayersJson = _currentSession.IdlePlayers.ToJson();
            await Clients.All.SendAsync("GameSessionStart", idlePlayersJson);
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return base.OnConnectedAsync();

            var userName = _userService.GetUserName(userId);

            var newUser = new SessionUser { Id = userId, Name = userName, ConnectionId = Context.ConnectionId };

            if (_currentSession == null)
            {       
                _queuedPlayers.Add(newUser);
                return base.OnConnectedAsync();
            }

            if(_queuedPlayers != null && _queuedPlayers.Count > 0) 
            {
                foreach(var item in _queuedPlayers)
                {
                    var isUserRegistered = _currentSession.IdlePlayers.Where(u => u.Id.Equals(item.Id)).FirstOrDefault() == null ? false : true;

                    if (!isUserRegistered)
                    {
                        _currentSession.IdlePlayers.Add(item);
                    }
                }
                _queuedPlayers.Clear();

            }

            if (_currentSession.IdlePlayers.Where(u => u.Id.Equals(userId)).FirstOrDefault() == null)
            {
                _currentSession.IdlePlayers.Add(newUser);
            }
            else
            {
                newUser.ConnectionId = Context.ConnectionId;
            }

            return base.OnConnectedAsync();

        }


        public override Task OnDisconnectedAsync(Exception? exception)
        {

            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
            {
                return base.OnDisconnectedAsync(exception);
            }           

            var queuedUser = _queuedPlayers.Where(u => u.Id == userId).FirstOrDefault();

            if ((_queuedPlayers.Count > 0) && (queuedUser != null))
            {
                _queuedPlayers.Remove(queuedUser);
            }

            if(_currentSession != null)
            {
                var idlePlayer = _currentSession.IdlePlayers.Where(u => u.Id == userId).FirstOrDefault();
                var activePlayer = _currentSession.PlayersList.Where(u => u.Id == userId).FirstOrDefault();

                if (idlePlayer != null)
                {
                    _currentSession.IdlePlayers.Remove(idlePlayer);
                }

                if (activePlayer != null)
                {
                    _currentSession.PlayersList.Remove(activePlayer);
                }

                foreach (var team in _currentSession.Teams)
                {
                    var teamPlayer = team.Players.Where(u => u.Id == userId).FirstOrDefault();

                    if (teamPlayer != null)
                    {
                        team.Players.Remove(teamPlayer);
                    }
                }
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
