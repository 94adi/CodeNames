using CodeNames.Repository;

namespace CodeNames.Repository
{
    public interface IUserRepository : IRepository<Microsoft.AspNetCore.Identity.IdentityUser>
    {
        public Microsoft.AspNetCore.Identity.IdentityUser? GetById(string id);
    }
}
