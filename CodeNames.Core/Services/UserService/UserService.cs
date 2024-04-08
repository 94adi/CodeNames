using CodeNames.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNames.Core.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userService;
        public UserService(IUserRepository userService) 
        { 
            _userService = userService;
        }
        public string GetUserName(string userId) => 
                _userService.GetById(userId)?.UserName;
    }
}
