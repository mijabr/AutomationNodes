using AutomationNodes.Core;
using System;

namespace AutomationPlayground.Worlds
{
    public class RandomShipScene : IScene
    {
        private readonly INodeCommander nodeCommander;
        private readonly Random random = new Random();

        public RandomShipScene(INodeCommander nodeCommander)
        {
            this.nodeCommander = nodeCommander;
        }

        public void Run(Clients clients)
        {
            for (var n = 0; n < 5; n++)
            {
                AddShip(clients);
            }
        }

        private void AddShip(Clients clients)
        {
            //nodeCommander.CreateNode<Ship>(clients)
            //    .FlyTo(NextXCoord(), NextYCoord())
            //    .FlyTo(NextXCoord(), NextYCoord())
            //    .FlyTo(NextXCoord(), NextYCoord())
            //    .FlyTo(NextXCoord(), NextYCoord())
            //    .FlyTo(NextXCoord(), NextYCoord())
            //    .FlyTo(NextXCoord(), NextYCoord())
            //    .FlyTo(NextXCoord(), NextYCoord())
            //    .FlyTo(NextXCoord(), NextYCoord())
            //    .Start();
        }

        private int NextXCoord() => random.Next(0, 1200);
        private int NextYCoord() => random.Next(0, 800);
    }
}
