using System.Collections.Generic;

namespace CodeNames;

public static class GameSessionDictioary
{
    private static IDictionary<Guid, LiveSession> _sessionDictionary;

    //key= uid + : + connectionId, val = live session id
    private static IDictionary<string, string> _liveUsersSession;

    private static readonly object _lock;

    static GameSessionDictioary()
    {
        _lock = new object();
        _liveUsersSession = new Dictionary<string, string>();
        _sessionDictionary = new Dictionary<Guid, LiveSession>();
    }

    public static LiveSession? GetUserSession(string userId, string connectionId)
    {
        lock (_lock) 
        {
            var key = userId + ":" + connectionId;

            if (_liveUsersSession.ContainsKey(key))
            {
                var sessionId = new Guid(_liveUsersSession[key]);
                if (_sessionDictionary.ContainsKey(sessionId))
                {
                    return _sessionDictionary[sessionId];
                }
            }

            return null;
        }
    }

    //use it!
    public static void AddUserToSession(string userId, string connectionId,  string sessionId)
    {
        lock (_lock) 
        {
            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(connectionId) || String.IsNullOrEmpty(sessionId))
                throw new ArgumentNullException("Bad argument");

            var userKey = userId + ":" + connectionId;

            if (_liveUsersSession.ContainsKey(userKey))
            {
                _liveUsersSession[userKey] = sessionId;
                return;
            }

            _liveUsersSession.Add(userKey, sessionId);
        }
    }

    public static LiveSession? GetSession(Guid id)
    {
        lock (_lock)
        {
            if (_sessionDictionary.ContainsKey(id))
            {
                return _sessionDictionary[id];
            }

            return null;
        }
    }

    public static void AddSession(LiveSession session)
    {
        lock (_lock)
        {
            if (session == null || session.SessionId == Guid.Empty)
                throw new ArgumentException("Bad argument");
            //make static session id
            _sessionDictionary.Add(session.SessionId, session);
            //add to LiveGameSession 
        }
    }

    public static void RemoveSession(LiveSession session)
    {
        lock (_lock)
        {
            if (session == null || session.SessionId == Guid.Empty)
                throw new ArgumentException("Bad argument");

            _sessionDictionary.Remove(session.SessionId);
        }
    }

    public static void RemoveUserFromSesion(string userId, string connectionId, string sessionId)
    {
        lock (_lock)
        {
            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(connectionId) || String.IsNullOrEmpty(sessionId))
                throw new ArgumentNullException("Bad argument");

            var userKey = userId + ":" + connectionId;

            if (_liveUsersSession.ContainsKey(userKey))
            {
                _liveUsersSession.Remove(userKey);
            }
        }
    }

    public static void RemoveUsersFromSession(string sessionId)
    {
        lock (_lock) 
        {
            var keysRemove = _liveUsersSession.Where(e => e.Value == sessionId)
                                  .Select(e => e.Key)
                                  .ToList();
            foreach (var key in keysRemove)
            {
                _liveUsersSession.Remove(key);
            }
        }
    }
}
