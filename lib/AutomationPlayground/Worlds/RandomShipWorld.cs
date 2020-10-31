using AutomationNodes.Core;
using AutomationPlayground.Nodes;
using System;

namespace AutomationPlayground.Worlds
{
    public class RandomShipWorld : WorldBase
    {
        public RandomShipWorld(WorldCatalogue worldCatalogue, WorldTime worldTime, string connectionId, IHubManager hubManager) : base(worldCatalogue, worldTime, connectionId, hubManager)
        {
        }

        public override void OnCreated()
        {
            base.OnCreated();

            for (var n = 0; n < 5; n++)
            {
                AddShip();
            }
        }

        private Random random = new Random();

        private void AddShip()
        {
            CreateNode<Ship>()
                .FlyTo(NextXCoord(), NextYCoord())
                .FlyTo(NextXCoord(), NextYCoord())
                .FlyTo(NextXCoord(), NextYCoord())
                .FlyTo(NextXCoord(), NextYCoord())
                .FlyTo(NextXCoord(), NextYCoord())
                .FlyTo(NextXCoord(), NextYCoord())
                .FlyTo(NextXCoord(), NextYCoord())
                .FlyTo(NextXCoord(), NextYCoord())
                .Start();
        }

        private int NextXCoord() => random.Next(0, 1200);
        private int NextYCoord() => random.Next(0, 800);
    }
}
