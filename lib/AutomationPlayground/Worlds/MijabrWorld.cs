using AutomationNodes.Core;
using AutomationPlayground.Scenes;
using System;
using System.Threading.Tasks;

namespace AutomationPlayground.Worlds
{
    public class MijabrWorld : World
    {
        private readonly INodeCommander nodeCommander;
        private readonly MijabrScene mijabr;
        private readonly StarFieldScene starField;
        private readonly MijabrProfile mijabrProfile;

        public MijabrWorld(
            INodeCommander nodeCommander,
            MijabrScene mijabr,
            StarFieldScene starFieldScene,
            MijabrProfile mijabrProfile)
        {
            this.nodeCommander = nodeCommander;
            this.mijabr = mijabr;
            this.starField = starFieldScene;
            this.mijabrProfile = mijabrProfile;
        }

        public override async Task OnMessage(string message)
        {
            if (message == "show-profile")
            {
                await Task.Run(() => mijabrProfile.ShowProfile(ConnectionId));
            }
            else if (message == "body")
            {
                await Task.Run(() => mijabrProfile.HideProfile(ConnectionId));
            }
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

            Start(starField);
            Start(mijabr, TimeSpan.FromSeconds(2));
            Start(mijabrProfile, TimeSpan.FromSeconds(4));
        }

        private void Start(IScene scene, TimeSpan? startTime = null)
        {
            Task.Run(async () => {
                if (startTime.HasValue)
                {
                    await Task.Delay(startTime.Value);
                }

                scene.Run(ConnectionId);
            });
        }
    }
}
