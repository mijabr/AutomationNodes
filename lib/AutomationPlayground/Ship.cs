using AutomationNodes.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomationNodes
{
    public class ShipImage : AutomationBase
    {
        public ShipImage(WorldCatalogue worldCatalogue, WorldBase world) : base(worldCatalogue, world)
        {
            Image = "ship-0001.svg";
        }

        public override string Type => "Img";
    }

    public class Ship : AutomationBase
    {
        private ShipImage shipImage;

        public Ship(WorldCatalogue worldCatalogue, WorldBase world) : base(worldCatalogue, world)
        {
            shipImage = new ShipImage(worldCatalogue, world);
            Children.Add(shipImage);
        }

        public override string Type => "Div";

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

        public override void OnCreated()
        {
            Speed = 250;
            worldCatalogue.SubscribeToNode(this, Id);
        }

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
        }
    }
}
