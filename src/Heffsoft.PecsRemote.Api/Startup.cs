using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heffsoft.PecsRemote.Api.Data;
using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using Heffsoft.PecsRemote.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Heffsoft.PecsRemote.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IServiceProvider ServiceProvider { get; }


        public Startup(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDataContext();
            services.AddApiServices();

            services.AddCors();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddJwtAuthentication(Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            lifetime.ApplicationStarted.Register(OnAppStarted);
            lifetime.ApplicationStopping.Register(OnAppStopping);
            lifetime.ApplicationStopped.Register(OnAppStopped);

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvc();
        }

        private void OnAppStopped()
        {
            INotificationService notificationService = ServiceProvider.GetService<INotificationService>();

            notificationService.AddNotification(new Notification()
            {
                Id = Guid.Empty,
                Type = NotificationType.System,
                Timestamp = DateTime.UtcNow,
                Title = "PECSRemote Stopped",
                Image = "/images/stopped.png",
                Content = "The PECSRemote platform has stopped.",
                Read = false
            });
        }

        private void OnAppStopping()
        {
        }

        private void OnAppStarted()
        {
            INotificationService notificationService = ServiceProvider.GetService<INotificationService>();

            notificationService.AddNotification(new Notification()
            {
                Id = Guid.Empty,
                Type = NotificationType.System,
                Timestamp = DateTime.UtcNow,
                Title = "PECSRemote Started",
                Image = "/images/started.png",
                Content = "The PECSRemote platform has started.",
                Read = false
            });
        }
    }
}
