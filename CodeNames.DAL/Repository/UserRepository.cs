
using CodeNames.Data;
using CodeNames.Repository;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
