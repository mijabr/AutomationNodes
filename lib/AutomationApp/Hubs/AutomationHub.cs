using AutomationNodes.Core;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutomationApp.Hubs
{
    public class AutomationHub : Hub
    {
        private readonly IHubUpstream hubUpstream;

        public AutomationHub(IHubUpstream hubUpstream)
        {
            this.hubUpstream = hubUpstream;
        }

        public async Task SendCapabilities(Caps caps)
        {
            await hubUpstream.OnConnect(Context.ConnectionId, caps);

            await Task.CompletedTask;
        }

        public async Task SendMessage(string message)
        {
            await hubUpstream.OnMessage(Context.ConnectionId, message);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await hubUpstream.OnDisconnect(Context.ConnectionId);

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

        public async Task Send(string connectionId, List<Dictionary<string, object>> messages)
        {
            await hubContext.Clients.Client(connectionId).SendAsync("AutomationMessage", messages);
        }
    }
}
