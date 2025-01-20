
namespace CodeNames.CodeNames.Core.Services.GameRoomService;

public class GameRoomService : IGameRoomService
{
    private readonly IGameRoomRepository _gameRoomRepository;

    public GameRoomService(IGameRoomRepository gameRoomRepository)
    {
        _gameRoomRepository = gameRoomRepository;
    }

    public IEnumerable<GameRoom> GetGameRooms()
    {
        return _gameRoomRepository.GetAll();
    }

    public GameRoom GetRoomByName(string name) =>
                _gameRoomRepository.GetRoomByName(name);

    public void InvalidateInvitationCode(int gameRoomId, Guid code)
    {
        var gameRoom = _gameRoomRepository.Get(gameRoomId);
        if(gameRoom == null)
        {
            throw new Exception("Could not find Game Room");
        }

        if(gameRoom.InvitationCode == code)
        {
            gameRoom.InvitationCode = Guid.NewGuid();
            _gameRoomRepository.Update(gameRoom);
        }
    }

    public bool IsGameRoomValid(string gameRoom)
    {
        var result = _gameRoomRepository.GetRoomByName(gameRoom) != null ? true : false;

        return result;
    }

    public bool IsInvitationCodeValid(string gameRoom, Guid code)
    {
        var gameRoomResult  = _gameRoomRepository.GetRoomByName(gameRoom);

        if (gameRoomResult == null) 
            throw new Exception("Game Room could not be found!");

        if (gameRoomResult.InvitationCode == code)
            return true;

        return false;
    }
}
