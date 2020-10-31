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
            worldCatalogue.SubscribeToNode(this, node.Id);
            hubManager.Send(ConnectionId, node.CreateMessage());
            node.OnCreated();

            return (T)node;
        }

        public void MoveNode(AutomationBase node)
        {
            hubManager.Send(ConnectionId, node.MoveMessage());

            worldCatalogue.AddFutureEvent(new TemporalEvent
            {
                EventName = "node-arrival",
                TriggerAt = worldTime.Time.Elapsed + node.HeadingEta,
                RegardingNode = node.Id,
                RegardingLocation = node.Heading
            });
        }

        public void RotateNode(AutomationBase node)
        {
            hubManager.Send(ConnectionId, node.RotateMessage());
        }

        public virtual void OnCreated()
        {
        }

        public void OnEvent(TemporalEvent t)
        {
            if (t.EventName == "node-arrival")
            {
                if (worldCatalogue.Nodes.TryGetValue(t.RegardingNode, out var node))
                {
                    node.Location = t.RegardingLocation;
                }
            }
        }
    }
}
