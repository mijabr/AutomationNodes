using AutomationNodes.Core;
using AutomationPlayground.Nodes;

namespace AutomationPlayground.Worlds
{
    public class ShipScene : SceneBase
    {
        public ShipScene(IWorld world) : base(world)
        {
        }

        public void Run()
        {
            World.CreateNode<Ship>()
                .SetSize("150px")
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
