using Heffsoft.PecsRemote.Api.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Heffsoft.PecsRemote.Api.Services
{
    public static class Extensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddSingleton<ICastService, CastService>();

            return services.AddTransient<IRandomService, RandomService>()
                           .AddTransient<IEventLogService, EventLogService>()
                           .AddTransient<INotificationService, NotificationService>()
                           .AddTransient<IUserService, UserService>()
                           .AddTransient<IHostService, HostService>()
                           .AddTransient<IContentService, ContentService>()
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
            using (SHA256 sha = new SHA256Managed())
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

        public static Boolean IsYouTubeUrl(this Uri uri)
        {
            return uri.ToString().IsYouTubeUrl();
        }

        public static Boolean IsYouTubeUrl(this String uri)
        {
            Regex ytRegex = new Regex(@"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$");
            return ytRegex.IsMatch(uri);
        }

        public static String Hash(this Stream stream)
        {
            using (SHA256 sha = new SHA256Managed())
            {
                Int64 loc = stream.Position;
                Byte[] hash = sha.ComputeHash(stream);
                stream.Position = loc;

                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        public static String Hash(this Byte[] data)
        {
            using (SHA256 sha = new SHA256Managed())
            {
                Byte[] hash = sha.ComputeHash(data);

                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        public static String Hash(this Object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (TextWriter tw = new StreamWriter(ms, Encoding.UTF8, 4096, true))
                {
                    using (JsonTextWriter jtw = new JsonTextWriter(tw))
                    {
                        JsonSerializer ser = new JsonSerializer();
                        ser.Serialize(jtw, obj);
                        jtw.Flush();
                    }
                }

                ms.Position = 0;
                using (SHA256 sha = new SHA256Managed())
                {
                    Byte[] hash = sha.ComputeHash(ms);

                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }

        public static Guid HashGuid(this Stream stream)
        {
            using (SHA256 sha = new SHA256Managed())
            {
                Int64 loc = stream.Position;
                Byte[] hash = sha.ComputeHash(stream);
                stream.Position = loc;

                Byte[] guid = new Byte[16];
                Int32 j = 0;
                for (Int32 i = 0; i < 32; i += 4)
                {
                    guid[j++] = (Byte)(hash[i + 0] ^ hash[i + 1]);
                    guid[j++] = (Byte)(hash[i + 2] ^ hash[i + 3]);
                }

                return new Guid(guid);
            }
        }

        public static Guid HashGuid(this Byte[] data)
        {
            using (SHA256 sha = new SHA256Managed())
            {
                Byte[] hash = sha.ComputeHash(data);

                Byte[] guid = new Byte[16];
                Int32 j = 0;
                for (Int32 i = 0; i < 32; i += 4)
                {
                    guid[j++] = (Byte)(hash[i + 0] ^ hash[i + 1]);
                    guid[j++] = (Byte)(hash[i + 2] ^ hash[i + 3]);
                }

                return new Guid(guid);
            }
        }

        public static Guid HashGuid(this Object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (TextWriter tw = new StreamWriter(ms, Encoding.UTF8, 4096, true))
                {
                    using (JsonTextWriter jtw = new JsonTextWriter(tw))
                    {
                        JsonSerializer ser = new JsonSerializer();
                        ser.Serialize(jtw, obj);
                        jtw.Flush();
                    }
                }

                ms.Position = 0;
                using (SHA256 sha = new SHA256Managed())
                {
                    Byte[] hash = sha.ComputeHash(ms);

                    Byte[] guid = new Byte[16];
                    Int32 j = 0;
                    for (Int32 i = 0; i < 32; i += 4)
                    {
                        guid[j++] = (Byte)(hash[i + 0] ^ hash[i + 1]);
                        guid[j++] = (Byte)(hash[i + 2] ^ hash[i + 3]);
                    }

                    return new Guid(guid);
                }
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

        public static Guid ToGuid(this IPAddress ip)
        {
            if (ip == null)
                throw new ArgumentNullException(nameof(ip));

            Byte[] bytes = ip.MapToIPv6().GetAddressBytes();
            return new Guid(bytes);
        }
    }
}
