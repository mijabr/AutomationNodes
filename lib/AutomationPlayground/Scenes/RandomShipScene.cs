using AutomationNodes.Core;
using AutomationPlayground.Nodes;
using System;

namespace AutomationPlayground.Worlds
{
    public class RandomShipScene : SceneBase
    {
        public RandomShipScene(IWorld world) : base(world)
        {
        }

        public void Run()
        {
            for (var n = 0; n < 5; n++)
            {
                AddShip();
            }
        }

        private Random random = new Random();

        private void AddShip()
        {
            World.CreateNode<Ship>()
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
