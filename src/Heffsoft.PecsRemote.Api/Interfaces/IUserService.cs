using Heffsoft.PecsRemote.Api.Data.Models;
using System;
using System.Net;

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
