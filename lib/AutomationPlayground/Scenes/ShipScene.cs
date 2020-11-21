using AutomationNodes.Core;
using AutomationPlayground.Nodes;

namespace AutomationPlayground.Worlds
{
    public class ShipScene : IScene
    {
        private readonly INodeCommander nodeCommander;

        public ShipScene(INodeCommander nodeCommander)
        {
            this.nodeCommander = nodeCommander;
        }

        public void Run(string connectionId)
        {
            nodeCommander.CreateNode<Ship>(connectionId)
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
