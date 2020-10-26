using AutomationApp.Hubs;
using AutomationNodes.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AutomationApp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();

            services.AddSingleton<ApplicationRunningToken>();
            services.AddSingleton<WorldTime>();
            services.AddSingleton<WorldCatalogue>();
            services.AddSingleton(typeof(IAutomationHubContext), typeof(AutomationHubContext));
            services.AddSingleton(typeof(IHubManager), typeof(HubManager));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<AutomationHub>("/automationhub");
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                var appToken = app.ApplicationServices.GetService(typeof(ApplicationRunningToken)) as ApplicationRunningToken;
                appToken.CancellationToken.Cancel();
            });
        }
    }
}
