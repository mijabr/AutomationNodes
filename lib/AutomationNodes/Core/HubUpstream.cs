using System.Threading;
using System.Threading.Tasks;

namespace AutomationNodes.Core
{
    public interface IHubUpstream
    {
        void RegisterWorld<T>(CancellationToken token) where T : IWorld;
        void OnConnect(string connectionId, Caps caps);
        void OnDisconnect(string connectionId);
        Task OnMessage(string connectionId, string message);
    }

    public class HubUpstream : IHubUpstream
    {
        private readonly INodeOrchestrator nodeOrchestrator;

        public HubUpstream(INodeOrchestrator nodeOrchestrator)
        {
            this.nodeOrchestrator = nodeOrchestrator;
        }

        public IWorld world { get; private set; }

        public void RegisterWorld<T>(CancellationToken token) where T : IWorld
        {
            world = nodeOrchestrator.CreateWorld<T>(token);
        }

        public void OnConnect(string connectionId, Caps caps)
        {
            nodeOrchestrator.Connect(connectionId, caps);
            world?.OnConnect(connectionId, caps);
        }

        public void OnDisconnect(string connectionId)
        {
            nodeOrchestrator.Disconnect(connectionId);
            world?.OnDisconnect(connectionId);
        }

        public async Task OnMessage(string connectionId, string message)
        {
            await world.OnMessage(connectionId, message);
        }
    }
}
