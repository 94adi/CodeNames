using CodeNames.Models;

namespace CodeNames.CodeNames.Core.Services.GameRoomService
{
    public interface IGameRoomService
    {
        bool IsInvitationCodeValid(string gameRoom, Guid code);

        bool IsGameRoomValid(string gameRoom);

        GameRoom GetRoomByName(string name);

        IEnumerable<GameRoom> GetGameRooms();
    }
}
