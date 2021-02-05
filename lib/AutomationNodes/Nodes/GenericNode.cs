using AutomationNodes.Core;

namespace AutomationNodes.Nodes
{
    public class GenericNode : Div, INode
    {
        private readonly INodeOrchestrator nodeOrchestrator;
        private readonly ITemporalEventQueue temporalEventQueue;
        private readonly IWorldTime worldTime;

        public GenericNode(
            INodeOrchestrator nodeOrchestrator,
            ITemporalEventQueue temporalEventQueue,
            IWorldTime worldTime)
        {
            this.nodeOrchestrator = nodeOrchestrator;
            this.temporalEventQueue = temporalEventQueue;
            this.worldTime = worldTime;
        }

        public override void OnCreated(Clients clients, object[] parameters)
        {
            base.OnCreated(clients, parameters);
            nodeOrchestrator.SetProperty(clients, new ClientNode(this), "position", "absolute");
        }
    }
}
