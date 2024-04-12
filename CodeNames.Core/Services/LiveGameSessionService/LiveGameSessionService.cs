using CodeNames.DAL.DALAbstraction;
using CodeNames.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNames.Core.Services.LiveGameSessionService
{
    public class LiveGameSessionService : ILiveGameSessionService
    {
        private readonly ILiveGameSessionRepository _liveGameSessionRepository;

        public LiveGameSessionService(ILiveGameSessionRepository liveGameSessionRepository)
        {
            _liveGameSessionRepository = liveGameSessionRepository;
        }

        public void AddEditLiveSession(LiveGameSession session)
        {
            _liveGameSessionRepository.AddEditLiveSession(session);
        }

        public LiveGameSession GetByGameRoom(int roomId) =>
                _liveGameSessionRepository.GetByGameRoom(roomId);

        public void Remove(LiveGameSession session)
        {
            _liveGameSessionRepository.Remove(session);
            _liveGameSessionRepository.Save();
        }

        public void Remove(int id)
        {
            _liveGameSessionRepository.Remove(id);
            _liveGameSessionRepository.Save();
        }

        public void Remove(IEnumerable<LiveGameSession> sessions)
        {
            _liveGameSessionRepository.RemoveRange(sessions);
            _liveGameSessionRepository.Save();
        }
    }
}
