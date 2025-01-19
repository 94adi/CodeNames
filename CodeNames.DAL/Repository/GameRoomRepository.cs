using CodeNames.Data;
using CodeNames.Models;
using Microsoft.EntityFrameworkCore;

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

        public void Update(GameRoom room)
        {
            _db.Attach(room);
            _db.Entry(room).State = EntityState.Modified;
            _db.SaveChanges();

        }
    }
}
