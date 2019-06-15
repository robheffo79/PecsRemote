using Heffsoft.PecsRemote.Api.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public static class Extensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            return services.AddTransient<IUserService, UserService>();
        }
    }
}
