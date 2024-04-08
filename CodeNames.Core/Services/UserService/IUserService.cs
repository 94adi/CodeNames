using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNames.Core.Services.UserService
{
    public interface IUserService
    {
        string GetUserName(string userId);
    }
}
