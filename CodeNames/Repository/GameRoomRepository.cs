using CodeNames.Data;
using CodeNames.Models;

namespace CodeNames.Repository
{
    public class GameRoomRepository : Repository<GameRoom>, IGameRoomRepository
    {
        private readonly AppDbContext _db;

        public GameRoomRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public GameRoom GetRoomByName(string name)
        {
            var result = base.GetFirstOrDefault(gr => gr.Name.Equals(name));
            return result;
        }
    }
}
