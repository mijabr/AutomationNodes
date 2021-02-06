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
        Task OnConnect(string connectionId);
        Task OnDisconnect(string connectionId);
        Task OnMessage(string connectionId, string message);
        CancellationToken CancellationToken { get; set; }
    }

    public class World : Div, IWorld
    {
        public virtual Task OnConnect(string connectionId) => Task.CompletedTask;

        public virtual Task OnDisconnect(string connectionId) => Task.CompletedTask;

        public virtual Task OnMessage(string connectionId, string message) => Task.CompletedTask;
        public CancellationToken CancellationToken { get ; set; }
    }
}
