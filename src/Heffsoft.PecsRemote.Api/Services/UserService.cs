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
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class UserService : IUserService
    {
        private const Int32 BAN_FOR_TIMEOUT_SECONDS = 1800;
        private const Int32 FAILED_ATTEMPT_LIMIT = 3;
        private const Int32 FAILED_ATTEMPT_WINDOW_SECONDS = 600;

        private readonly IDataContext dataContext;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IRandomService randomService;
        private readonly INotificationService notificationService;
        private readonly IHostService hostService;
        private readonly IDataRepository<User> userRepo;
        private readonly IDataRepository<BannedAddress> bannedAddressRepo;

        private static List<FailedAttempt> failedAttempts = new List<FailedAttempt>();

        public UserService(IDataContext dataContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IRandomService randomService, INotificationService notificationService, IHostService hostService)
        {
            this.dataContext = dataContext;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this.randomService = randomService;
            this.notificationService = notificationService;
            this.hostService = hostService;

            this.userRepo = this.dataContext.GetRepository<User>();
            this.bannedAddressRepo = this.dataContext.GetRepository<BannedAddress>();
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

        public String AuthenticateUser(String username, String password, IPAddress clientIP)
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
            {
                CheckBan(clientIP);
                return null;
            }

            if (password.Hash(user.Salt) != user.HashedPassword)
            {
                CheckBan(clientIP);
                return null;
            }

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

        private void CheckBan(IPAddress clientIP)
        {
            lock(failedAttempts)
            {
                failedAttempts.Add(new FailedAttempt() { ClientIP = clientIP, Timestamp = DateTime.Now });
                failedAttempts.RemoveAll(a => (DateTime.Now - a.Timestamp).TotalSeconds > FAILED_ATTEMPT_WINDOW_SECONDS);

                IEnumerable<FailedAttempt> forClient = failedAttempts.Where(a => a.ClientIP == clientIP);
                if(forClient.Count() > FAILED_ATTEMPT_LIMIT)
                {
                    Ban(clientIP);
                }
            }
        }

        private void Ban(IPAddress clientIP)
        {
            Int32 banTime = (Int32)(BAN_FOR_TIMEOUT_SECONDS * (randomService.NextDouble() * 0.1D));
            String msg = $"Too many failed login attempts from IP Address '{clientIP}'. Address has been blacklisted for {banTime} seconds.";
            notificationService.AddNotification(NotificationType.Security, "Failed Login Attempt Ban", msg, "/images/banhammer.png");

            Guid id = clientIP.ToGuid();
            BannedAddress banned = bannedAddressRepo.Get(id);

            if(banned != null)
            {
                banned.BanCount++;
                banned.LastBanned = DateTime.Now;
                banned.UnbanAt = DateTime.Now.AddSeconds(banTime);
                bannedAddressRepo.Update(banned);
            }
            else
            {
                banned = new BannedAddress()
                {
                    Id = id,
                    LastBanned = DateTime.Now,
                    UnbanAt = DateTime.Now.AddSeconds(banTime),
                    IPAddress = clientIP.ToString(),
                    BanCount = 1
                };

                bannedAddressRepo.Insert<Guid>(banned);
            }

            hostService.BlacklistIP(clientIP);
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

            user.Id = userRepo.Insert<Int32>(user);
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
