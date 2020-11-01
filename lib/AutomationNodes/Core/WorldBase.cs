using System;
using System.Collections.Generic;

namespace AutomationNodes.Core
{
    public class WorldBase : ITemporalEventHandler
    {
        private readonly WorldCatalogue worldCatalogue;
        private readonly WorldTime worldTime;
        private readonly IHubManager hubManager;

        public WorldBase(
            WorldCatalogue worldCatalogue,
            WorldTime worldTime,
            string connectionId,
            IHubManager hubManager)
        {
            this.worldCatalogue = worldCatalogue;
            this.worldTime = worldTime;
            ConnectionId = connectionId;
            this.hubManager = hubManager;
        }

        public Guid Id { get; } = Guid.NewGuid();

        public string ConnectionId { get; }

        public T CreateNode<T>(params object[] parameters) where T : AutomationBase
        {
            return DoCreateNode<T>(null, parameters);
        }

        public T CreateChildNode<T>(AutomationBase parent, params object[] parameters) where T : AutomationBase
        {
            return DoCreateNode<T>(parent, parameters);
        }

        private T DoCreateNode<T>(AutomationBase parent, params object[] parameters) where T : AutomationBase
        {
            var p = new List<object> { worldCatalogue, this };
            p.AddRange(parameters);
            var t = Activator.CreateInstance(typeof(T), p.ToArray());

            if (!(t is AutomationBase node)) throw new Exception("Node must be type of AutomationBase");

            node.Parent = parent;
            worldCatalogue.Nodes.Add(node.Id, node);
            hubManager.Send(ConnectionId, node.CreationMessage());
            node.OnCreated();

            return (T)node;
        }

        public void SetProperty(string name, string value, Guid? nodeId = null)
        {
            hubManager.Send(ConnectionId, new Dictionary<string, object>
            {
                { "message", "SetProperty" },
                { "id", nodeId ?? Id },
                { "name", name },
                { "value", value }
            });
        }

        internal void SetTransition(Dictionary<string, string> transitionProperties, TimeSpan timeSpan, Guid nodeId)
        {
            hubManager.Send(ConnectionId, new Dictionary<string, object>
            {
                { "message", "SetTransition" },
                { "id", nodeId },
                { "properties", transitionProperties },
                { "duration", timeSpan.TotalMilliseconds }
            });
        }

        public TimeSpan Time => worldTime.Time.Elapsed;

        public virtual void OnCreated()
        {
        }

        public void OnEvent(TemporalEvent t)
        {
        }
    }
}
