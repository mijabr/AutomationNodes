using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationNodes.Core
{
    public interface IHubDownstream
    {
        void Send(string connectionId, Dictionary<string, object> message);
        void Start(CancellationToken token);
    }

    public class HubDownstream : IHubDownstream
    {
        private readonly IAutomationHubContext automationHubContext;

        public HubDownstream(IAutomationHubContext automationHubContext)
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

                foreach (var clientMessages in clientsMessagesToSend)
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
