using AutomationNodes.Core;

namespace AutomationNodes.Nodes
{
    public class DivNode : AutomationBase
    {
        public DivNode(WorldBase world) : base(world)
        {
        }

        public override string Type => "Div";
    }
}
