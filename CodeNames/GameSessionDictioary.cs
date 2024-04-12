using CodeNames.Models;

namespace CodeNames
{
    public static class GameSessionDictioary
    {
        private static IDictionary<Guid, LiveSession> _sessionDictionary = new Dictionary<Guid, LiveSession>();

        //key= uid + : + sid, val = live session id
        private static IDictionary<string, string> _liveUsersSession = new Dictionary<string, string>();

        public static LiveSession? GetUserSession(string userId, string connectionId)
        {
            var key = userId + ":" + connectionId;

            if (_liveUsersSession.ContainsKey(key))
            {
                var sessionId = new Guid(_liveUsersSession[key]);
                return _sessionDictionary[sessionId];
            }

            return null;
        }

        //use it!
        public static void AddUserToSession(string userId, string connectionId,  string sessionId)
        {
            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(connectionId) ||  String.IsNullOrEmpty(sessionId))
                throw new ArgumentNullException("Bad argument");

            var userKey = userId + ":" + connectionId;

            if (_liveUsersSession.ContainsKey(userKey))
            {
                _liveUsersSession[userKey] = sessionId;
                return;
            }

            _liveUsersSession.Add(userKey, sessionId);
        }

        public static LiveSession? GetSession(Guid id)
        {
            if (_sessionDictionary.ContainsKey(id))
            {
                return _sessionDictionary[id];
            }

            return null;
        }

        public static void AddSession(LiveSession session)
        {
            if (session == null || session.SessionId == Guid.Empty)
                throw new ArgumentException("Bad argument");
            //make static session id
            _sessionDictionary.Add(session.SessionId, session);
            //add to LiveGameSession 
        }

        public static void RemoveSession(LiveSession session)
        {
            if (session == null || session.SessionId == Guid.Empty)
                throw new ArgumentException("Bad argument");

            _sessionDictionary.Remove(session.SessionId);
        }
    }
}
