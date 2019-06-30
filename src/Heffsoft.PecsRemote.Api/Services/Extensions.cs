using Heffsoft.PecsRemote.Api.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public static class Extensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddSingleton<ICastService, CastService>();

            return services.AddTransient<IRandomService, RandomService>()
                           .AddTransient<IUserService, UserService>()
                           .AddTransient<IHostService, HostService>()
                           .AddTransient<IMediaService, MediaService>();
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            Byte[] key = Encoding.UTF8.GetBytes(configuration.GetValue<String>("Authentication:JwtSecret"));
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            return services;
        }

        public static String Hash(this String value, String salt)
        {
            using(SHA256 sha = new SHA256Managed())
            {
                Byte[] bytes = Encoding.UTF8.GetBytes($"{salt}{value}");
                Byte[] hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", String.Empty).ToLowerInvariant();
            }
        }

        public static String GetLast(this String text, Int32 tailLength)
        {
            if (tailLength > text.Length)
                return text;

            return text.Substring(text.Length - tailLength);
        }
    }
}
