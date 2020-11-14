using AutomationNodes.Core;

namespace AutomationNodes.Nodes
{
    public class Div : AutomationBase
    {
        public Div(WorldBase world) : base(world)
        {
        }

        public override string Type => "Div";
    }
}
