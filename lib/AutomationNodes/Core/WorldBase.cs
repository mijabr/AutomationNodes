using System;
using System.Collections.Generic;

namespace AutomationNodes.Core
{
    public interface IWorld
    {
        T CreateNode<T>(params object[] parameters) where T : AutomationBase;
        object CreateNode(Type type, params object[] parameters);
        T CreateChildNode<T>(AutomationBase parent, params object[] parameters) where T : AutomationBase;
        object CreateChildNode(Type type, AutomationBase parent, params object[] parameters);
        void SetProperty(string name, string value, Guid? nodeId = null);
        void SetTransition(Dictionary<string, string> transitionProperties, TimeSpan duration, Guid nodeId);
        TimeSpan Time { get; }
        void AddFutureEvent(TemporalEvent temporalEvent);
        void SubscribeToNode(ITemporalEventHandler temporalEventHandler, Guid regardingNodeId);
    }

    public class WorldBase : IWorld
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
            return (T)DoCreateNode(typeof(T), null, parameters);
        }

        public object CreateNode(Type type, params object[] parameters)
        {
            return DoCreateNode(type, null, parameters);
        }

        public T CreateChildNode<T>(AutomationBase parent, params object[] parameters) where T : AutomationBase
        {
            return (T)DoCreateNode(typeof(T), parent, parameters);
        }

        public object CreateChildNode(Type type, AutomationBase parent, params object[] parameters)
        {
            return DoCreateNode(type, parent, parameters);
        }

        private object DoCreateNode(Type type, AutomationBase parent, params object[] parameters)
        {
            var p = new List<object> { this };
            p.AddRange(parameters);
            var t = Activator.CreateInstance(type, p.ToArray());

            if (!(t is AutomationBase node)) throw new Exception("Node must be type of AutomationBase");

            node.Parent = parent;
            worldCatalogue.Nodes.Add(node.Id, node);
            hubManager.Send(ConnectionId, node.CreationMessage());
            node.OnCreated();

            return node;
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

        public void SetTransition(Dictionary<string, string> transitionProperties, TimeSpan duration, Guid nodeId)
        {
            hubManager.Send(ConnectionId, new Dictionary<string, object>
            {
                { "message", "SetTransition" },
                { "id", nodeId },
                { "properties", transitionProperties },
                { "duration", duration.TotalMilliseconds }
            });
        }

        public TimeSpan Time => worldTime.Time.Elapsed;

        public void AddFutureEvent(TemporalEvent temporalEvent)
        {
            worldCatalogue.AddFutureEvent(temporalEvent);
        }

        public void SubscribeToNode(ITemporalEventHandler temporalEventHandler, Guid regardingNodeId)
        {
            worldCatalogue.SubscribeToNode(temporalEventHandler, regardingNodeId);
        }

        public virtual void OnCreated()
        {
        }
    }
}
