using AutomationNodes.Core;
using AutomationPlayground.Nodes;

namespace AutomationPlayground.Worlds
{
    public class ShipWorld : WorldBase
    {
        public ShipWorld(WorldCatalogue worldCatalogue, WorldTime worldTime, string connectionId, IHubManager hubManager) : base(worldCatalogue, worldTime, connectionId, hubManager)
        {
        }

        public override void OnCreated()
        {
            base.OnCreated();

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
