using AutomationNodes.Core;
using AutomationPlayground.Scenes;

namespace AutomationPlayground.Worlds
{
    public class MijabrWorld : World
    {
        private readonly INodeCommander nodeCommander;
        private readonly MijabrScene mijabrScene;
        private readonly RocketElephantScene birdFlyAttemptScene;

        public MijabrWorld(
            INodeCommander nodeCommander,
            MijabrScene mijabrScene,
            RocketElephantScene birdFlyAttemptScene)
        {
            this.nodeCommander = nodeCommander;
            this.mijabrScene = mijabrScene;
            this.birdFlyAttemptScene = birdFlyAttemptScene;
        }

        public override void OnCreated(object[] parameters)
        {
            base.OnCreated(parameters);

            nodeCommander.SetProperty(this, "position", "relative");
            nodeCommander.SetProperty(this, "width", "900px");
            nodeCommander.SetProperty(this, "height", "900px");
            nodeCommander.SetProperty(this, "color", "white");
            nodeCommander.SetProperty(this, "background-color", "black");
            nodeCommander.SetProperty(this, "overflow", "hidden");

            mijabrScene.Run(ConnectionId);
            birdFlyAttemptScene.Run(ConnectionId);
        }
    }
}
