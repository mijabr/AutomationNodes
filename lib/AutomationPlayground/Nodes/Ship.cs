using AutomationNodes.Core;
using AutomationNodes.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomationPlayground.Nodes
{
    public class Ship : Div, ITemporalEventHandler
    {
        private readonly INodeCommander nodeCommander;
        private readonly ITemporalEventQueue temporalEventQueue;
        private readonly IWorldTime worldTime;

        public Ship(INodeCommander nodeCommander, ITemporalEventQueue temporalEventQueue, IWorldTime worldTime)
        {
            this.nodeCommander = nodeCommander;
            this.temporalEventQueue = temporalEventQueue;
            this.worldTime = worldTime;
        }

        private Image shipImage;

        public override void OnCreated(object[] parameters)
        {
            base.OnCreated(parameters);
            shipImage = nodeCommander.CreateChildNode<Image>(this, "assets/ship-0001.svg");
            nodeCommander.SetProperty(this, "position", "absolute");
            SetSize("50px");
            Speed = 250;
            temporalEventQueue.SubscribeToNode(this, Id);
        }

        public Ship SetSize(string size)
        {
            nodeCommander.SetProperty(this, "width", size);
            nodeCommander.SetProperty(this, "height", size);
            nodeCommander.SetProperty(shipImage, "width", size);
            nodeCommander.SetProperty(shipImage, "height", size);
            return this;
        }

        public Point Location { get; set; } = new Point();
        public Point Heading { get; private set; } = new Point();
        public TimeSpan HeadingEta { get; private set; }
        public double Speed { get; set; } = 15;

        public List<Point> Waypoints { get; } = new List<Point>();

        public Ship FlyTo(int x, int y)
        {
            return FlyTo(new Point(x, y));
        }

        public Ship FlyTo(Point point)
        {
            Waypoints.Add(point);
            return this;
        }

        public Ship FlyNext(int x, int y)
        {
            return FlyTo(new Point(Waypoints.Last().X + x, Waypoints.Last().Y + y));
        }

        public void OnTemporalEvent(TemporalEvent t)
        {
            if (t.EventName == "node-arrival")
            {
                Location = t.RegardingLocation;
            }

            Next();
        }

        public void Start()
        {
            waypointIndex = 0;
            if (Waypoints?.Count > 0)
            {
                Location = Waypoints[0];
                nodeCommander.SetProperty(this, "left", $"{Location.X}");
                nodeCommander.SetProperty(this, "top", $"{Location.Y}");
                Next();
            }
        }

        private void Next()
        {
            var lastWaypoint = Waypoints[waypointIndex];
            waypointIndex++;
            if (waypointIndex >= Waypoints.Count)
            {
                waypointIndex = 0;
            }
            var rotation = lastWaypoint.DirectionTo(Waypoints[waypointIndex]);
            nodeCommander.SetProperty(shipImage, "transform", $"rotate({rotation}deg)");
            MoveTo(Waypoints[waypointIndex]);
        }

        private int waypointIndex;

        private void MoveTo(Point location, TimeSpan? time = null)
        {
            Heading = location;
            HeadingEta = time ?? TimeSpan.FromMilliseconds(Location.DistanceTo(Heading) / Speed * 1000);
            var transitionProperties = new Dictionary<string, string> {
                {"left", $"{Heading.X}px"},
                {"top", $"{Heading.Y}px"},
            };
            nodeCommander.SetTransition(this, transitionProperties, HeadingEta);

            temporalEventQueue.AddFutureEvent(new TemporalEvent
            {
                EventName = "node-arrival",
                TriggerAt = worldTime.Time.Elapsed + HeadingEta,
                RegardingNode = Id,
                RegardingLocation = Heading
            });
        }
    }
}
