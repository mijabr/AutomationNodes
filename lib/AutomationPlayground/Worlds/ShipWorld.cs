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

            SetProperty("width", "1000px");
            SetProperty("height", "1000px");
            SetProperty("color", "white");
            SetProperty("background-color", "black");
            SetProperty("overflow", "hidden");

            CreateNode<Ship>()
                .FlyTo(new Point(300, 300))
                .FlyNext(100, 0)
                .FlyNext(100, 100)
                .FlyNext(0, 100)
                .FlyNext(-100, 100)
                .FlyNext(-100, 0)
                .FlyNext(-100, -100)
                .FlyNext(0, -100)
                .FlyNext(100, -100)
                .Start();

        }
    }
}
