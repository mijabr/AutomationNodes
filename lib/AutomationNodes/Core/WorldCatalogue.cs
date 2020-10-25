using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationNodes.Core
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

        public List<WorldBase> Worlds { get; } = new List<WorldBase>();

        public Dictionary<Guid, AutomationBase> Nodes { get; } = new Dictionary<Guid, AutomationBase>();

        public T CreateWorld<T>(string connectionId) where T : WorldBase
        {
            var t = Activator.CreateInstance(typeof(T), new object[] { this, worldTime, automationHubContext, connectionId });

            if (!(t is WorldBase world)) throw new Exception("Worlds must be based on WorldBase class");

            Worlds.Add(world);

            world.OnCreated();

            return (T)world;
        }

        public async Task StartTemporalEventQueue(CancellationToken token)
        {
            var stopwatch = Stopwatch.StartNew();
            while (!token.IsCancellationRequested)
            {
                var startCount = events.Count;

                List<TemporalEvent> processedEvents;
                lock (syncObj)
                {
                    var now = worldTime.Time.Elapsed;
                    processedEvents = events.Where(e => now >= e.TriggerAt).ToList();
                    processedEvents.ForEach(t => events.Remove(t));
                }

                processedEvents.ForEach(e =>
                {
                    if (subscriptions.TryGetValue(e.RegardingNode, out var regardingNodeSubscriptions))
                    {
                        regardingNodeSubscriptions.ForEach(handler => handler.OnEvent(e));
                    }
                });

                var endCount = events.Count;
                //Console.WriteLine($"Processed in {startCount-endCount}/{startCount} {stopwatch.Elapsed}");
                stopwatch.Restart();
                await Task.Delay(20);
            }
        }

        internal async Task MoveNode(WorldBase world, AutomationBase node)
        {
            await automationHubContext.Send(world.ConnectionId, node);

            AddFutureEvent(new TemporalEvent
            {
                EventName = "node-arrival",
                TriggerAt = worldTime.Time.Elapsed + TimeSpan.FromMilliseconds(node.HeadingEta),
                RegardingNode = node.Id,
                RegardingLocation = node.Heading
            });
        }

        private readonly List<TemporalEvent> events = new List<TemporalEvent>();
        private static object syncObj = new object();

        public void AddFutureEvent(TemporalEvent temporalEvent)
        {
            lock (syncObj)
            {
                events.Add(temporalEvent);
            }
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

    public class WorldBase : ITemporalEventHandler
    {
        private readonly WorldCatalogue worldCatalogue;
        private readonly WorldTime worldTime;
        private readonly IAutomationHubContext automationHubContext;

        public WorldBase(
            WorldCatalogue worldCatalogue,
            WorldTime worldTime,
            IAutomationHubContext automationHubContext,
            string connectionId)
        {
            this.worldCatalogue = worldCatalogue;
            this.worldTime = worldTime;
            this.automationHubContext = automationHubContext;
            ConnectionId = connectionId;
        }

        public string ConnectionId { get; }

        public T CreateNode<T>() where T : AutomationBase
        {
            var t = Activator.CreateInstance(typeof(T), new object[] { worldCatalogue, this });

            if (!(t is AutomationBase node)) throw new Exception("Node must be based on AutomationBase class");

            worldCatalogue.Nodes.Add(node.Id, node);
            worldCatalogue.SubscribeToNode(this, node.Id);
            node.OnCreated();

            return (T)node;
        }

        public virtual void OnCreated()
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
        void OnEvent(TemporalEvent t);
    }

    public interface ICreatable
    {
        void OnCreated();
    }
}
