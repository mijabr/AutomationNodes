using AutomationNodes;
using AutomationNodes.Core;
using System;

namespace AutomationPlayground
{
    public class RandomShipWorld : WorldBase
    {
        public RandomShipWorld(WorldCatalogue worldCatalogue, WorldTime worldTime, string connectionId) : base(worldCatalogue, worldTime, connectionId)
        {
        }

        public override void OnCreated()
        {
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
