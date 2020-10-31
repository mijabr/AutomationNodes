using AutomationNodes.Core;
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
            shipImage = CreateNode<ShipImage>();
            Speed = 250;
            worldCatalogue.SubscribeToNode(this, Id);
        }

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

        public Ship Fly(int x, int y)
        {
            return FlyTo(new Point(Waypoints.Last().X + x, Waypoints.Last().Y + y));
        }

        private int waypointIndex;

        public override void OnEvent(TemporalEvent t)
        {
            Next();
        }

        public void Start()
        {
            waypointIndex = 0;
            if (Waypoints?.Count > 0)
            {
                Location = Waypoints[0];
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
            shipImage.Rotation = lastWaypoint.DirectionTo(Waypoints[waypointIndex]);
            MoveTo(Waypoints[waypointIndex]);
            world.RotateNode(shipImage);
        }
    }
}
