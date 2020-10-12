using System.Threading.Tasks;

namespace AutomationNodes
{
    public class Ship : AutomationBase
    {
        public Ship(WorldCatalogue worldCatalogue, World world) : base(worldCatalogue, world)
        {
        }

        public override string Image => "ship-0001.svg";

        private Point[] waypoints = new[]
        {
            new Point(100, 100),
            new Point(200, 100),
            new Point(200, 200),
            new Point(100, 200)
        };

        private int waypointIndex = 0;

        public override void OnCreated()
        {
            Speed = 250;
            Location = waypoints[0];
            worldCatalogue.SubscribeToNode(this, Id);
            Next();
        }

        public override void OnEvent(TemporalEvent t)
        {
            Next();
        }

        private void Next()
        {
            waypointIndex++;
            if (waypointIndex >= waypoints.Length)
            {
                waypointIndex = 0;
            }

            Task.Run(() => world.MoveNode(this, waypoints[waypointIndex]));
        }
    }
}
