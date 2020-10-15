using AutomationNodes.Core;
using System.Threading.Tasks;

namespace AutomationNodes
{
    public class Ship : AutomationBase
    {
        public Ship(WorldCatalogue worldCatalogue, World world) : base(worldCatalogue, world)
        {
        }

        public override string Image => "ship-0001.svg";

        private Point[] waypoints;
        public Point[] Waypoints
        {
            get { return waypoints; }
            set { waypoints = value; Start(); }
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

        private void Start()
        {
            waypointIndex = 0;
            if (waypoints?.Length > 0)
            {
                Location = waypoints[0];
                Next();
            }
        }

        private void Next()
        {
            waypointIndex++;
            if (waypointIndex >= waypoints.Length)
            {
                waypointIndex = 0;
            }

            MoveTo(waypoints[waypointIndex]);
        }
    }
}
