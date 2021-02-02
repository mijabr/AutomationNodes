using AutomationNodes.Core;
using AutomationPlayground.Scenes;
using System;
using System.Collections.Generic;
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
            StarFieldScene starField,
            MijabrProfile mijabrProfile)
        {
            this.nodeCommander = nodeCommander;
            this.mijabr = mijabr;
            this.starField = starField;
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

        private ClientContext clientContext = new();

        public override void OnCreated(object[] parameters)
        {
            base.OnCreated(parameters);

            clientContext.ConnectionId = ConnectionId;

            if (parameters?.Length > 0)
            {
                clientContext.Caps = parameters[0] as Caps;
                if (clientContext.Caps.isMobile)
                {
                    clientContext.ImageScaling = 2.0;
                    clientContext.FontScaling = 1.3;
                }
            }

            nodeCommander.SetProperties(this, new Dictionary<string, string>
            {
                ["position"] = "relative",
                ["width"] = "100%",
                ["height"] = "100%",
                ["color"] = "white",
                ["background-color"] = "black",
                ["overflow"] = "hidden",
                ["font-family"] = "verdana",
                ["font-size"] = clientContext.ScaledFont(1)
            });

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

                scene.Run(clientContext);
            });
        }
    }
}
