using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationNodes
{
    public class WorldCatalogue
    {
        private readonly WorldTime worldTime;
        private readonly IAutomationHubContext automationHubContext;

        public WorldCatalogue(
            WorldTime worldTime,
            IAutomationHubContext automationHubContext)
        {
            this.worldTime = worldTime;
            this.automationHubContext = automationHubContext;
        }

        public List<World> Worlds { get; } = new List<World>();

        public Dictionary<Guid, AutomationBase> Nodes { get; } = new Dictionary<Guid, AutomationBase>();

        public World CreateNewWorld(string connectionId)
        {
            var world = new World(this, worldTime, automationHubContext, connectionId);
            Worlds.Add(world);

            return world;
        }

        public async Task StartTemporalEventQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var processedEvents = new List<TemporalEvent>();
                var now = worldTime.Time.Elapsed;
                events.ForEach(t =>
                {
                    if (now >= t.TriggerAt)
                    {
                        if (subscriptions.TryGetValue(t.RegardingNode, out var regardingNodeSubscriptions))
                        {
                            regardingNodeSubscriptions.ForEach(handler => handler.OnEvent(t));
                        }

                        processedEvents.Add(t);
                    }
                });

                processedEvents.ForEach(t => events.Remove(t));

                await Task.Delay(20);
            }
        }

        private readonly List<TemporalEvent> events = new List<TemporalEvent>();

        public void AddFutureEvent(TemporalEvent temporalEvent)
        {
            events.Add(temporalEvent);
        }

        private readonly Dictionary<Guid, List<ITemporalEventHandler>> subscriptions = new Dictionary<Guid, List<ITemporalEventHandler>>();

        public void SubscribeToNode(ITemporalEventHandler temporalEventHandler, Guid regardingNodeId)
        {
            if (!subscriptions.TryGetValue(regardingNodeId, out var regardingNodeSubscriptions))
            {
                regardingNodeSubscriptions = new List<ITemporalEventHandler>();
                subscriptions.Add(regardingNodeId, regardingNodeSubscriptions);
            }

            regardingNodeSubscriptions.Add(temporalEventHandler);
        }
    }

    public class World : ITemporalEventHandler
    {
        private readonly WorldCatalogue worldCatalogue;
        private readonly WorldTime worldTime;
        private readonly IAutomationHubContext automationHubContext;
        private readonly string connectionId;

        public World(
            WorldCatalogue worldCatalogue,
            WorldTime worldTime,
            IAutomationHubContext automationHubContext,
            string connectionId)
        {
            this.worldCatalogue = worldCatalogue;
            this.worldTime = worldTime;
            this.automationHubContext = automationHubContext;
            this.connectionId = connectionId;
        }

        public T CreateNode<T>() where T: AutomationBase
        {
            var t = Activator.CreateInstance(typeof(T), new object[] { worldCatalogue, this });

            if (!(t is AutomationBase node)) throw new Exception("Node must be based on AutomationBase class");

            worldCatalogue.Nodes.Add(node.Id, node);
            worldCatalogue.SubscribeToNode(this, node.Id);
            node.OnCreated();

            return (T)node;
        }

        public async Task MoveNode(AutomationBase node, Point location)
        {
            node.Heading = location;
            node.HeadingEta = node.Location.DistanceTo(node.Heading) / node.Speed * 1000;

            await automationHubContext.Send(connectionId, node);

            worldCatalogue.AddFutureEvent(new TemporalEvent
            {
                EventName = "node-arrival",
                TriggerAt = worldTime.Time.Elapsed + TimeSpan.FromMilliseconds(node.HeadingEta),
                RegardingNode = node.Id,
                RegardingLocation = node.Heading
            });
        }

        public void OnCreated()
        {
        }

        public void OnEvent(TemporalEvent t)
        {
            if (worldCatalogue.Nodes.TryGetValue(t.RegardingNode, out var node))
            {
                node.Location = t.RegardingLocation;
            }
        }
    }

    public class TemporalEvent
    {
        public string EventName { get; set; }
        public TimeSpan TriggerAt { get; set; }
        public Guid RegardingNode { get; set; }
        public Point RegardingLocation { get; set; }
    }

    public interface ITemporalEventHandler
    {
        void OnCreated();
        void OnEvent(TemporalEvent t);
    }
}
