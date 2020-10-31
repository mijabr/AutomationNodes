using AutomationNodes.Core;

namespace AutomationNodes.Nodes
{
    public class DivNode : AutomationBase
    {
        public DivNode(WorldCatalogue worldCatalogue, WorldBase world) : base(worldCatalogue, world)
        {
        }

        public override string Type => "Div";
    }
}
