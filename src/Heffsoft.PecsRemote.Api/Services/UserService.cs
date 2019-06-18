﻿using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IDataContext dataContext;
        private readonly IConfiguration configuration;

        public UserService(IDataContext dataContext, IConfiguration configuration)
        {
            this.dataContext = dataContext;
            this.configuration = configuration;
        }

        public String AuthenticateUser(String username, String password)
        {
            IDataRepository<User> repo = dataContext.GetRepository<User>();
            User user = repo.Find("`Username` = @Username", new { Username = username }).SingleOrDefault();
            if (user == null)
                return null;

            if (password.Hash(user.Salt) != user.HashedPassword)
                return null;

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            Byte[] key = Encoding.UTF8.GetBytes(configuration.GetValue<String>("Authentication:JwtSecret"));

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
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
