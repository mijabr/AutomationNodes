using AutomationNodes.Core;

namespace AutomationPlayground.Worlds
{
    public class ShipScene : IScene
    {
        private readonly INodeCommander nodeCommander;

        public ShipScene(INodeCommander nodeCommander)
        {
            this.nodeCommander = nodeCommander;
        }

        public void Run(Clients clients)
        {
            //nodeCommander.CreateNode<Ship>(clients)
            //    .SetSize("150px")
            //    .FlyTo(new Point(300, 300))
            //    .FlyNext(100, 0)
            //    .FlyNext(100, 100)
            //    .FlyNext(0, 100)
            //    .FlyNext(-100, 100)
            //    .FlyNext(-100, 0)
            //    .FlyNext(-100, -100)
            //    .FlyNext(0, -100)
            //    .FlyNext(100, -100)
            //    .Start();
        }
    }
}
