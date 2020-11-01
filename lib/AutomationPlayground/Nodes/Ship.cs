﻿using AutomationNodes.Core;
using AutomationNodes.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomationPlayground.Nodes
{
    public class ShipImage : ImageNode
    {
        public ShipImage(WorldCatalogue worldCatalogue, WorldBase world) : base(worldCatalogue, world, "ship-0001.svg")
        {
        }
    }

    public class Ship : DivNode
    {
        private ShipImage shipImage;

        public Ship(WorldCatalogue worldCatalogue, WorldBase world) : base(worldCatalogue, world)
        {
        }

        public override void OnCreated()
        {
            SetProperty("position", "absolute");
            SetProperty("width", "50px");
            SetProperty("height", "50px");
            shipImage = CreateNode<ShipImage>();
            shipImage.SetProperty("width", "50px");
            shipImage.SetProperty("height", "50px");

            Speed = 250;
            worldCatalogue.SubscribeToNode(this, Id);
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

        public override void OnEvent(TemporalEvent t)
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
                SetProperty("left", $"{Location.X}");
                SetProperty("top", $"{Location.Y}");
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
            shipImage.SetProperty("transform", $"rotate({rotation}deg)");
            MoveTo(Waypoints[waypointIndex]);
        }

        private int waypointIndex;

        private void MoveTo(Point location, TimeSpan? time = null)
        {
            Heading = location;
            HeadingEta = time ?? TimeSpan.FromMilliseconds(Location.DistanceTo(Heading) / Speed * 1000);
            SetTransition(new Dictionary<string, string> {
                {"left", $"{Heading.X}px"},
                {"top", $"{Heading.Y}px"},
            }, HeadingEta
            );

            worldCatalogue.AddFutureEvent(new TemporalEvent
            {
                EventName = "node-arrival",
                TriggerAt = world.Time + HeadingEta,
                RegardingNode = Id,
                RegardingLocation = Heading
            });
        }
    }
}
