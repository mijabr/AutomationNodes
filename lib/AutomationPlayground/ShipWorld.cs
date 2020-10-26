using AutomationNodes;
using AutomationNodes.Core;

namespace AutomationPlayground
{
    public class ShipWorld : WorldBase
    {
        public ShipWorld(WorldCatalogue worldCatalogue, WorldTime worldTime, string connectionId) : base(worldCatalogue, worldTime, connectionId)
        {
        }

        public override void OnCreated()
        {
            CreateNode<Ship>()
                .FlyTo(new Point(300, 300))
                .Fly(100, 0)
                .Fly(100, 100)
                .Fly(0, 100)
                .Fly(-100, 100)
                .Fly(-100, 0)
                .Fly(-100, -100)
                .Fly(0, -100)
                .Fly(100, -100)
                .Start();

        }
    }
}
