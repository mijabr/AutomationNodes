using AutomationNodes.Core;
using AutomationPlayground.Scenes;

namespace AutomationPlayground.Worlds
{
    public class MijabrWorld : World
    {
        private readonly INodeCommander nodeCommander;
        private readonly MijabrScene mijabrScene;
        private readonly RocketElephantScene rocketElephantScene;
        private readonly RandomShipScene randomShipScene;
        private readonly ShipScene shipScene;

        public MijabrWorld(
            INodeCommander nodeCommander,
            MijabrScene mijabrScene,
            RocketElephantScene rocketElephantScene,
            RandomShipScene randomShipScene,
            ShipScene shipScene)
        {
            this.nodeCommander = nodeCommander;
            this.mijabrScene = mijabrScene;
            this.rocketElephantScene = rocketElephantScene;
            this.randomShipScene = randomShipScene;
            this.shipScene = shipScene;
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
            rocketElephantScene.Run(ConnectionId);
            //shipScene.Run(ConnectionId);
            //randomShipScene.Run(ConnectionId);
        }
    }
}
