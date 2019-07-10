using Heffsoft.PecsRemote.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IUserService
    {
        User CurrentUser { get; }

        User GetUser(String username);
        String AuthenticateUser(String username, String password, IPAddress clientIP);
        void CreateUser(String username, String password);
    }
}
