using AutomationNodes;
using AutomationNodes.Core;
using AutomationPlayground;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public async Task Send(string connectionId, List<AutomationBase> nodes)
        {
            await hubContext.Clients.Client(connectionId).SendAsync("AutomationMessage", nodes);
        }
    }

    public class HubManager : IHubManager
    {
        private readonly IAutomationHubContext automationHubContext;

        public HubManager(IAutomationHubContext automationHubContext)
        {
            this.automationHubContext = automationHubContext;
        }

        public void Send(string connectionId, AutomationBase node)
        {
            if (clientsMessages.TryGetValue(connectionId, out var clientMessages))
            {
                clientMessages.Add(node);
            }
            else
            {
                var newClientMessages = new List<AutomationBase>();
                clientsMessages.Add(connectionId, newClientMessages);
                newClientMessages.Add(node);
            }
        }

        public void Start(CancellationToken token)
        {
            Task.Run(() => Run(token));
        }

        private async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Dictionary<string, List<AutomationBase>> clientsMessagesToSend;
                lock (lockObj)
                {
                    clientsMessagesToSend = clientsMessages;
                    clientsMessages = new Dictionary<string, List<AutomationBase>>();
                }

                foreach(var clientMessages in clientsMessagesToSend)
                {
                    await automationHubContext.Send(clientMessages.Key, clientMessages.Value);
                }

                await Task.Delay(50);
            }
        }

        private Dictionary<string, List<AutomationBase>> clientsMessages = new Dictionary<string, List<AutomationBase>>();

        private object lockObj = new object();
    }
}
