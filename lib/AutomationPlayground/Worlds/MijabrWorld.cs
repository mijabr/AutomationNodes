using AutomationNodes.Core;
using AutomationPlayground.Scenes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutomationPlayground.Worlds
{
    public class MijabrWorld : World
    {
        private readonly INodeOrchestrator nodeOrchestrator;
        private readonly MijabrScene mijabr;
        private readonly StarFieldScene starField;
        private readonly MijabrProfile mijabrProfile;

        public MijabrWorld(
            INodeOrchestrator nodeOrchestrator,
            MijabrScene mijabr,
            StarFieldScene starField,
            MijabrProfile mijabrProfile)
        {
            this.nodeOrchestrator = nodeOrchestrator;
            this.mijabr = mijabr;
            this.starField = starField;
            this.mijabrProfile = mijabrProfile;
        }

        public override async Task OnConnect(string connectionId)
        {
            await Task.Run(() => {
                var clients = Clients.WithIds(connectionId);
                Start(mijabr, clients, TimeSpan.FromSeconds(2));
                Start(mijabrProfile, clients, TimeSpan.FromSeconds(4));
            });
        }

        private void Start(IScene scene, Clients clients, TimeSpan afterDelay)
        {
            Task.Run(async () => {
                await Task.Delay(afterDelay);
                scene.Run(clients);
            });
        }

        public override async Task OnMessage(string connectionId, string message)
        {
            if (message == "show-profile")
            {
                await Task.Run(() => mijabrProfile.ShowProfile(Clients.WithIds(connectionId)));
            }
            else if (message == "body")
            {
                await Task.Run(() => mijabrProfile.HideProfile(Clients.WithIds(connectionId)));
            }
        }

        public override void OnCreated(Clients clients, object[] parameters)
        {
            base.OnCreated(clients, parameters);

            nodeOrchestrator.SetProperties(clients, new ClientNode(this), new Dictionary<string, string>
            {
                ["position"] = "relative",
                ["width"] = "100%",
                ["height"] = "100%",
                ["color"] = "white",
                ["background-color"] = "black",
                ["overflow"] = "hidden",
                ["font-family"] = "verdana",
                ["font-size"] = "1em"
            });

            starField.Run(Clients.All);
        }
    }
}
