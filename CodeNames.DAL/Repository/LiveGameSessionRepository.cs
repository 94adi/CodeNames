using CodeNames.DAL.DALAbstraction;
using CodeNames.Data;
using CodeNames.Models;
using CodeNames.Repository;
using Microsoft.EntityFrameworkCore;


namespace CodeNames.DAL.Repository
{
    public class LiveGameSessionRepository : Repository<LiveGameSession>, ILiveGameSessionRepository
    {
        private readonly AppDbContext _db;

        public LiveGameSessionRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public void AddEditLiveSession(LiveGameSession session)
        {
            if (session == null) 
                throw new ArgumentNullException("Invalid argument");

            var sessionObj = _db.LiveGameSession.FirstOrDefault(l => l.Id == session.Id);

            if(sessionObj != null)
            {
                sessionObj.SessionId = session.SessionId;
                sessionObj.GameRoomId = session.GameRoomId;
                _db.Entry(sessionObj).State = EntityState.Modified;
                base.Save();
                return;
            }

            base.Add(session);
            base.Save();
        }

        public LiveGameSession GetByGameRoom(int roomId)
        {
            var result = base.GetFirstOrDefault(lgs => lgs.GameRoomId == roomId);
            return result;
        }
    }
}
