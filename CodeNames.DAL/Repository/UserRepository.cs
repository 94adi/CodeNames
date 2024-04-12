using CodeNames.Data;
using CodeNames.Repository;

namespace CodeNames.DAL.Repository
{
    public class UserRepository : Repository<Microsoft.AspNetCore.Identity.IdentityUser>, IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public Microsoft.AspNetCore.Identity.IdentityUser? GetById(string id)
        {
            return _db.Users.FirstOrDefault(u => u.Id == id);
        }
    }
}
