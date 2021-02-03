using AutomationNodes.Core;

namespace AutomationNodes.Nodes
{
    public class GenericNode : Div, INode
    {
        private readonly INodeCommander nodeCommander;
        private readonly ITemporalEventQueue temporalEventQueue;
        private readonly IWorldTime worldTime;

        public GenericNode(
            INodeCommander nodeCommander,
            ITemporalEventQueue temporalEventQueue,
            IWorldTime worldTime)
        {
            this.nodeCommander = nodeCommander;
            this.temporalEventQueue = temporalEventQueue;
            this.worldTime = worldTime;
        }


        public override void OnCreated(object[] parameters)
        {
            base.OnCreated(parameters);
            nodeCommander.SetProperty(this, "position", "absolute");
        }
    }
}
