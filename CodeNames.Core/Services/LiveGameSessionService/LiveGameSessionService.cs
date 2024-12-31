using CodeNames.DAL.DALAbstraction;

namespace CodeNames.Core.Services.LiveGameSessionService;

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
