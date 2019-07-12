using Heffsoft.PecsRemote.Api.Data;
using Heffsoft.PecsRemote.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
            });

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

            app.UseForwardedHeaders();

            if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); }

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseAuthentication();
            app.UseMvc();
        }

        private void OnAppStopped()
        {
        }

        private void OnAppStopping()
        {
            //INotificationService notificationService = ServiceProvider.GetService<INotificationService>();

            //notificationService.AddNotification(new Notification()
            //{
            //    Id = Guid.Empty,
            //    Type = NotificationType.System,
            //    Timestamp = DateTime.UtcNow,
            //    Title = "PECSRemote Stopped",
            //    Image = "/images/stopped.png",
            //    Content = "The PECSRemote platform has stopped.",
            //    Read = false
            //});
        }

        private void OnAppStarted()
        {
            //Task.Run(async () =>
            //{
            //    INotificationService notificationService = ServiceProvider.GetService<INotificationService>();

            //    while(notificationService == null)
            //    {
            //        await Task.Delay(100);
            //        notificationService = ServiceProvider.GetService<INotificationService>();
            //    }

            //    notificationService.AddNotification(new Notification()
            //    {
            //        Id = Guid.Empty,
            //        Type = NotificationType.System,
            //        Timestamp = DateTime.UtcNow,
            //        Title = "PECSRemote Started",
            //        Image = "/images/started.png",
            //        Content = "The PECSRemote platform has started.",
            //        Read = false
            //    });
            //});
        }
    }
}
