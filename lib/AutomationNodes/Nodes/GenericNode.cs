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
            //body = nodeCommander.CreateChildNode<Image>(this, "assets/flying-bird-body.png");
            //leftWing = nodeCommander.CreateChildNode<Image>(this, "assets/flying-bird-left-wing.png");
            //rightWing = nodeCommander.CreateChildNode<Image>(this, "assets/flying-bird-right-wing.png");
            //nodeCommander.SetProperty(body, "z-index", "1");
            //SetPropertyForAll("position", "absolute");
            //SetSize(parameters[0] as string, parameters[1] as string);
            //Flap(TimeSpan.FromSeconds(1));
        }
    }
}
