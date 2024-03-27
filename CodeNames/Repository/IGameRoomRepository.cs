using CodeNames.Models;

namespace CodeNames.Repository
{
    public interface IGameRoomRepository : IRepository<GameRoom>
    {
        public GameRoom GetRoomByName(string name);
    }
}
