using CodeNames.Models;
using CodeNames.Repository;

namespace CodeNames.CodeNames.Core.Services.GameRoomService
{
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
}
