using System.Threading;
using System.Threading.Tasks;

namespace AutomationNodes.Core
{
    public interface IHubUpstream
    {
        void RegisterWorld<T>(CancellationToken token) where T : IWorld;
        Task OnConnect(string connectionId, Caps caps);
        Task OnDisconnect(string connectionId);
        Task OnMessage(string connectionId, string message);
    }

    public class HubUpstream : IHubUpstream
    {
        private readonly INodeOrchestrator nodeOrchestrator;
        private readonly IConnectedClients connectedClients;

        public HubUpstream(
            INodeOrchestrator nodeOrchestrator,
            IConnectedClients connectedClients)
        {
            this.nodeOrchestrator = nodeOrchestrator;
            this.connectedClients = connectedClients;
        }

        public IWorld world { get; private set; }

        public void RegisterWorld<T>(CancellationToken token) where T : IWorld
        {
            world = nodeOrchestrator.CreateWorld<T>(token);
        }

        public async Task OnConnect(string connectionId, Caps caps)
        {
            connectedClients.Connect(connectionId, caps);
            nodeOrchestrator.OnConnect(connectionId);
            await world?.OnConnect(connectionId);
        }

        public async Task OnDisconnect(string connectionId)
        {
            connectedClients.Disconnect(connectionId);
            await world?.OnDisconnect(connectionId);
        }

        public async Task OnMessage(string connectionId, string message)
        {
            await world?.OnMessage(connectionId, message);
        }
    }
}
