using AutomationNodes;
using AutomationNodes.Core;
using AutomationPlayground;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace AutomationApp.Hubs
{
    public class AutomationHub : Hub
    {
        private readonly WorldCatalogue worldCatalogue;

        public AutomationHub(WorldCatalogue worldCatalogue)
        {
            this.worldCatalogue = worldCatalogue;
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var world = worldCatalogue.CreateWorld<RandomShipWorld>(Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }

    public class AutomationHubContext : IAutomationHubContext
    {
        private readonly IHubContext<AutomationHub> hubContext;

        public AutomationHubContext(IHubContext<AutomationHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public async Task Send(string connectionId, AutomationBase node)
        {
            await hubContext.Clients.Client(connectionId).SendAsync("AutomationMessage", new[] { node });
        }
    }
}
