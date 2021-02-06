using AutomationNodes.Nodes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationNodes.Core
{
    public interface IWorld
    {
        public Guid Id { get; }
        void OnCreated(Clients clients, object[] parameters);
        void OnConnect(string connectionId, Caps caps);
        void OnDisconnect(string connectionId);
        Task OnMessage(string connectionId, string message);
        CancellationToken CancellationToken { get; set; }
    }

    public class World : Div, IWorld
    {
        public virtual void OnConnect(string connectionId, Caps caps) { }

        public virtual void OnDisconnect(string connectionId) { }

        public virtual Task OnMessage(string connectionId, string message) => Task.CompletedTask;
        public CancellationToken CancellationToken { get ; set; }
    }
}
