using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IRandomService randomService;
        private readonly IDataRepository<User> userRepo;

        public UserService(IDataContext dataContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IRandomService randomService)
        {
            this.dataContext = dataContext;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this.randomService = randomService;

            this.userRepo = this.dataContext.GetRepository<User>();
        }

        private static User currentUser = null;
        public User CurrentUser
        {
            get
            {
                if (currentUser == null)
                {
                    String token = httpContextAccessor.HttpContext.GetTokenAsync("access_token").Result;
                    if (String.IsNullOrWhiteSpace(token))
                        return null;

                    Byte[] key = Encoding.UTF8.GetBytes(configuration.GetValue<String>("Authentication:JwtSecret"));
                    JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

                    TokenValidationParameters validations = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };

                    ClaimsPrincipal claims = handler.ValidateToken(token, validations, out SecurityToken tokenSecure);
                    JwtSecurityToken jwtToken = (JwtSecurityToken)tokenSecure;

                    String username = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Name).SingleOrDefault()?.Value;
                    if (username != null)
                        currentUser = GetUser(username);
                }

                return currentUser;
            }
        }

        public String AuthenticateUser(String username, String password)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            if (String.IsNullOrWhiteSpace(username))
                throw new ArgumentException($"{nameof(username)} is empty.");

            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (String.IsNullOrWhiteSpace(password))
                throw new ArgumentException($"{nameof(password)} is empty.");

            User user = userRepo.Find("`Username` = @Username", new { Username = username }).SingleOrDefault();
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

        public void CreateUser(String username, String password)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            if (String.IsNullOrWhiteSpace(username))
                throw new ArgumentException($"{nameof(username)} is empty.");

            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (String.IsNullOrWhiteSpace(password))
                throw new ArgumentException($"{nameof(password)} is empty.");

            User existing = GetUser(username);
            if (existing != null)
                throw new InvalidOperationException($"{nameof(username)} is already in use.");

            String salt = randomService.NextSalt(64);
            String hashedPassword = password.Hash(salt);

            User user = new User()
            {
                Username = username,
                Salt = salt,
                HashedPassword = hashedPassword
            };

            user.Id = userRepo.Insert(user);
        }

        public User GetUser(String username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            if (String.IsNullOrWhiteSpace(username))
                throw new ArgumentException($"{nameof(username)} is empty.");

            return userRepo.Find("`Username` == @Username", new { Username = username }).SingleOrDefault();
        }
    }
}
