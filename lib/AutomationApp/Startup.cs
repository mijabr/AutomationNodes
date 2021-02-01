using AutomationApp.Hubs;
using AutomationNodes.Core;
using AutomationNodes.Core.Compile;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Reflection;

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
            services.AddSingleton(typeof(IAutomationHubContext), typeof(AutomationHubContext));
            services.AddSingleton(typeof(ITemporalEventQueue), typeof(TemporalEventQueue));
            services.AddSingleton(typeof(IHubManager), typeof(HubManager));
            services.AddSingleton(typeof(IWorldTime), typeof(WorldTime));
            services.AddSingleton(typeof(IScriptTokenizer), typeof(ScriptTokenizer));
            services.AddSingleton(typeof(ISceneCompiler), typeof(SceneCompiler));
            services.AddSingleton(typeof(ISceneActioner), typeof(SceneActioner));
            services.AddSingleton(typeof(INodeCommander), typeof(NodeCommander));
            services.AddSingleton(typeof(IOpeningModule), typeof(OpeningModule));
            services.AddSingleton(typeof(IParameterModule), typeof(ParameterModule));
            services.AddSingleton(typeof(IConstructionModule), typeof(ConstructionModule));
            services.AddSingleton(typeof(ISetFunctionModule), typeof(SetFunctionModule));
            services.AddSingleton(typeof(ITransitionFunctionModule), typeof(TransitionFunctionModule));
            services.AddSingleton(typeof(IKeyframeModule), typeof(KeyframeModule));
            services.AddSingleton(typeof(IFunctionModule), typeof(FunctionModule));
            services.AddSingleton(typeof(IClassModule), typeof(ClassModule));
            services.AddSingleton(typeof(Worlds));

            services.AddAutomationArtifactsFromAssembly(Assembly.Load("AutomationNodes"));
            services.AddAutomationArtifactsFromAssembly(Assembly.Load("AutomationPlayground"));
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

    public static class IServicesExtension
    {
        public static void AddAutomationArtifactsFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract &&
                    (typeof(INode).IsAssignableFrom(t) || typeof(IScene).IsAssignableFrom(t)))
                .ToList()
                .ForEach(t => {
                    services.AddTransient(t);
                });
        }
    }
}
