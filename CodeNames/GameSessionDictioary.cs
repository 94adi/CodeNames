using CodeNames.Models;

namespace CodeNames
{
    public static class GameSessionDictioary
    {
        private static IDictionary<Guid, LiveSession> _sessionDictionary = new Dictionary<Guid, LiveSession>();

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

            _sessionDictionary.Add(session.SessionId, session);
        }
    }
}
