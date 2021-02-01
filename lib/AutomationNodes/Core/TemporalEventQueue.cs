using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationNodes.Core
{
    public class TemporalEvent
    {
        public string EventName { get; set; }
        public TimeSpan TriggerAt { get; set; }
        public Guid RegardingNode { get; set; }
        public Point RegardingLocation { get; set; }
        public Action Action { get; set; }
    }

    public interface ITemporalEventHandler
    {
        void OnTemporalEvent(TemporalEvent t);
    }

    public interface ITemporalEventQueue
    {
        void SubscribeToNode(ITemporalEventHandler temporalEventHandler, Guid regardingNodeId);
        void AddFutureEvent(TemporalEvent temporalEvent);
    }

    public class TemporalEventQueue : ITemporalEventQueue
    {
        public TemporalEventQueue(IWorldTime worldTime)
        {
            this.worldTime = worldTime;
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
                    e.Action?.Invoke();

                    if (subscriptions.TryGetValue(e.RegardingNode, out var regardingNodeSubscriptions))
                    {
                        regardingNodeSubscriptions.ForEach(handler => handler.OnTemporalEvent(e));
                    }
                });

                var endCount = events.Count;
                //Console.WriteLine($"Processed in {startCount-endCount}/{startCount} {stopwatch.Elapsed}");
                stopwatch.Restart();
                await Task.Delay(10);
            }
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
        private readonly IWorldTime worldTime;

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
}
