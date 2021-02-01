using AutomationNodes.Core;
using AutomationNodes.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationPlayground.Scenes
{
    public class MijabrProfile : IScene
    {
        private readonly INodeCommander nodeCommander;

        public MijabrProfile(INodeCommander nodeCommander)
        {
            this.nodeCommander = nodeCommander;
        }

        private Image icon;
        private Div profile;

        public void Run(string connectionId)
        {
            CreateIcon(connectionId);
        }

        public void ShowProfile(string connectionId)
        {
            CreateProfile(connectionId);

            nodeCommander.SetTransition(icon, new Dictionary<string, string>
            {
                ["left"] = "5%",
                ["top"] = "5%"
            }, TimeSpan.FromMilliseconds(400));

            nodeCommander.SetTransition(profile, new Dictionary<string, string>
            {
                ["opacity"] = "1",
            }, TimeSpan.FromMilliseconds(400));
        }

        internal void HideProfile(string connectionId)
        {
            if (profile == null) return;

            nodeCommander.SetTransition(profile, new Dictionary<string, string>
            {
                ["opacity"] = "0",
            }, TimeSpan.FromMilliseconds(400), true);
            profile = null;

            nodeCommander.SetTransition(icon, new Dictionary<string, string>
            {
                ["left"] = "75%",
                ["top"] = "7%"
            }, TimeSpan.FromMilliseconds(400));
        }

        private void CreateIcon(string connectionId)
        {
            if (icon != null) return;

            icon = nodeCommander.CreateNode<Image>(connectionId, "assets/profile-2020-04-15_low-transarent-58.png");
            nodeCommander.SetProperties(icon, new Dictionary<string, string>
            {
                ["width"] = "0.01%",
                ["height"] = "0.01%",
                ["left"] = "50%",
                ["top"] = "50%",
                ["onclick"] = "show-profile"
            });
            nodeCommander.SetTransition(icon, new Dictionary<string, string>
            {
                ["transition-timing-function"] = "cubic-bezier(0.9,0,0.95,1)",
                ["width"] = "4%",
                ["height"] = "4%",
                ["left"] = "75%",
                ["top"] = "7%"
            }, TimeSpan.FromSeconds(5));
        }

        private void CreateProfile(string connectionId)
        {
            if (profile != null) return;

            profile = nodeCommander.CreateNode<Div>(connectionId);
            nodeCommander.SetProperties(profile, new Dictionary<string, string>
            {
                ["position"] = "absolute",
                ["left"] = "12%",
                ["top"] = "6%",
                ["width"] = "88%",
                ["height"] = "88%",
                ["opacity"] = "0"
            });
            nodeCommander.CreateChildNode<Text>(profile, "View this project on GitHub");
            nodeCommander.CreateChildNode<Text>(profile, "https://github.com/mijabr/AutomationNodes");
        }
    }
}
