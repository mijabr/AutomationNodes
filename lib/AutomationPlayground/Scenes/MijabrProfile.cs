using AutomationNodes.Core;
using AutomationNodes.Nodes;
using System;
using System.Collections.Generic;

namespace AutomationPlayground.Scenes
{
    public class MijabrProfile : IScene
    {
        private readonly INodeOrchestrator nodeOrchestrator;

        public MijabrProfile(INodeOrchestrator nodeOrchestrator)
        {
            this.nodeOrchestrator = nodeOrchestrator;
        }

        public void Run(Clients clients)
        {
            CreateIcon(clients);
        }

        private const string profileIcon = "profile-icon";
        private const string profilePanel = "profile-panel";

        private void CreateIcon(Clients clients)
        {
            var icon = nodeOrchestrator.CreateNamedNode(typeof(Image), clients, profileIcon, "assets/profile-2020-04-15_low-transarent-58.png");
            nodeOrchestrator.SetProperties(clients, icon, new Dictionary<string, string>
            {
                ["width"] = "0.01%",
                ["left"] = "50%",
                ["top"] = "50%",
                ["onclick"] = "show-profile"
            });
            nodeOrchestrator.SetTransition(clients, icon, new Dictionary<string, string>
            {
                ["transition-timing-function"] = "cubic-bezier(0.9,0,0.95,1)",
                ["width"] = "4%",
                ["left"] = "75%",
                ["top"] = "7%"
            }, TimeSpan.FromSeconds(5));
        }

        public void ShowProfile(Clients clients)
        {
            var icon = nodeOrchestrator.GetNamedNode(clients, profileIcon);
            var profile = CreateProfile(clients);

            nodeOrchestrator.SetTransition(clients, icon, new Dictionary<string, string>
            {
                ["left"] = "5%",
                ["top"] = "5%"
            }, TimeSpan.FromMilliseconds(400));

            nodeOrchestrator.SetTransition(clients, profile, new Dictionary<string, string>
            {
                ["opacity"] = "1",
            }, TimeSpan.FromMilliseconds(400));
        }

        private IClientNode CreateProfile(Clients clients)
        {
            var profile = nodeOrchestrator.GetNamedNode(clients, profilePanel);
            if (profile != null) return profile;

            profile = nodeOrchestrator.CreateNamedNode(typeof(Div), clients, profilePanel);
            nodeOrchestrator.SetProperties(clients, profile, new Dictionary<string, string>
            {
                ["position"] = "absolute",
                ["left"] = "12%",
                ["top"] = "6%",
                ["width"] = "88%",
                ["opacity"] = "0"
            });
            nodeOrchestrator.CreateChildNode(typeof(Text), clients, profile, "View this project on GitHub");
            nodeOrchestrator.CreateChildNode(typeof(Text), clients, profile, "https://github.com/mijabr/AutomationNodes");

            return profile;
        }

        public void HideProfile(Clients clients)
        {
            var profile = nodeOrchestrator.GetNamedNode(clients, profilePanel);
            if (profile == null) return;

            nodeOrchestrator.SetTransition(clients, profile, new Dictionary<string, string>
            {
                ["opacity"] = "0",
            }, TimeSpan.FromMilliseconds(400), true);

            var icon = nodeOrchestrator.GetNamedNode(clients, profileIcon);
            nodeOrchestrator.SetTransition(clients, icon, new Dictionary<string, string>
            {
                ["left"] = "75%",
                ["top"] = "7%"
            }, TimeSpan.FromMilliseconds(400));
        }
    }
}
