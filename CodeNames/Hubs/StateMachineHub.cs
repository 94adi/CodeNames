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
        private static IDictionary<string, string> _queuedPlayers = new Dictionary<string, string>();
        public async void ReceiveSessionId(string sessionId)
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
                foreach (KeyValuePair<string, string> item in _queuedPlayers)
                {
                    if (!_currentSession.IdlePlayers.ContainsKey(item.Key))
                    {
                        _currentSession.IdlePlayers.Add(item);
                    }
                }
                _queuedPlayers.Clear();

            }

            _currentSession.SessionState = SessionState.Start;
            await Clients.All.SendAsync("GameSessionStart", _currentSession.IdlePlayers.ToJson());
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null) 
                return base.OnConnectedAsync();

            if (_currentSession == null)
            {
                _queuedPlayers.Add(userId, Context.ConnectionId);
                return base.OnConnectedAsync();
            }

            if(_queuedPlayers != null && _queuedPlayers.Count > 0) 
            {
                foreach(KeyValuePair<string, string> item in _queuedPlayers)
                {
                    if(!_currentSession.IdlePlayers.ContainsKey(item.Key))
                    {
                        _currentSession.IdlePlayers.Add(item);
                    }
                }
                _queuedPlayers.Clear();

            }

            if (!_currentSession.IdlePlayers.ContainsKey(userId))
            {
                _currentSession.IdlePlayers.Add(new KeyValuePair<string, string>(userId, Context.ConnectionId));
            }
            else
            {
                _currentSession.IdlePlayers[userId] = Context.ConnectionId;
            }

            return base.OnConnectedAsync();

        }


        public override Task OnDisconnectedAsync(Exception? exception)
        {

            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
                return base.OnDisconnectedAsync(exception);

            if(_queuedPlayers.Count > 0 && _queuedPlayers.ContainsKey(userId))
            {
                _queuedPlayers.Remove(userId);
            }

            if(_currentSession != null)
            {
                if (_currentSession.IdlePlayers.ContainsKey(userId))
                {
                    _currentSession.IdlePlayers.Remove(userId);
                }

                if (_currentSession.PlayersList.ContainsKey(userId))
                {
                    _currentSession.PlayersList.Remove(userId);
                }

                foreach (var team in _currentSession.Teams)
                {
                    if (team.Players.ContainsKey(userId))
                    {
                        team.Players.Remove(userId);
                    }
                }
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
