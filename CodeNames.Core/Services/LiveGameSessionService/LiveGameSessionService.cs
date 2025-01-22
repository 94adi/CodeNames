using CodeNames.DAL.DALAbstraction;
using CodeNames.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace CodeNames.Core.Services.LiveGameSessionService;

public class LiveGameSessionService : ILiveGameSessionService
{
    private readonly ILiveGameSessionRepository _liveGameSessionRepository;
    private readonly IServiceProvider _serviceProvider;

    public LiveGameSessionService(ILiveGameSessionRepository liveGameSessionRepository,
        IServiceProvider serviceProvider)
    {
        _liveGameSessionRepository = liveGameSessionRepository;
        _serviceProvider = serviceProvider;
    }

    public void AddEditLiveSession(LiveGameSession session)
    {
        _liveGameSessionRepository.AddEditLiveSession(session);
    }

    public LiveGameSession GetByGameRoom(int roomId) 
    {
        //using var scope = _serviceProvider.CreateScope();
        //var repository = scope.ServiceProvider.GetRequiredService<ILiveGameSessionRepository>();

        //return repository.GetByGameRoom(roomId);
        //TEST IT OUT
        return _liveGameSessionRepository.GetByGameRoom(roomId);
    }

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
