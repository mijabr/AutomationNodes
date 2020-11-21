using AutomationNodes;
using AutomationNodes.Core;
using AutomationPlayground.Worlds;
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
        private readonly INodeCommander nodeCommander;

        public AutomationHub(INodeCommander nodeCommander)
        {
            this.nodeCommander = nodeCommander;
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            nodeCommander.CreateWorld<MijabrWorld>(Context.ConnectionId);
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

        public async Task Send(string connectionId, List<Dictionary<string, object>> messages)
        {
            await hubContext.Clients.Client(connectionId).SendAsync("AutomationMessage", messages);
        }
    }

    public class HubManager : IHubManager
    {
        private readonly IAutomationHubContext automationHubContext;

        public HubManager(IAutomationHubContext automationHubContext)
        {
            this.automationHubContext = automationHubContext;
        }

        public void Send(string connectionId, Dictionary<string, object> message)
        {
            lock (lockObj)
            {
                if (clientsMessages.TryGetValue(connectionId, out var clientMessages))
                {
                    clientMessages.Add(message);
                }
                else
                {
                    var newClientMessages = new List<Dictionary<string, object>>();
                    clientsMessages.Add(connectionId, newClientMessages);
                    newClientMessages.Add(message);
                }
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
                Dictionary<string, List<Dictionary<string, object>>> clientsMessagesToSend;
                lock (lockObj)
                {
                    clientsMessagesToSend = clientsMessages;
                    clientsMessages = new Dictionary<string, List<Dictionary<string, object>>>();
                }

                foreach(var clientMessages in clientsMessagesToSend)
                {
                    await automationHubContext.Send(clientMessages.Key, clientMessages.Value);
                }

                await Task.Delay(10);
            }
        }

        private Dictionary<string, List<Dictionary<string, object>>> clientsMessages = new Dictionary<string, List<Dictionary<string, object>>>();

        private object lockObj = new object();
    }
}
