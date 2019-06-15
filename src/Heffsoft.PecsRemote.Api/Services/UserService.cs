using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class UserService : IUserService
    {
        public Boolean AuthenticateUser(String username, String password)
        {
            throw new NotImplementedException();
        }

        public void CreateUser(String username, String password, IEnumerable<String> roles)
        {
            throw new NotImplementedException();
        }

        public User GetUser(String username)
        {
            throw new NotImplementedException();
        }
    }
}
