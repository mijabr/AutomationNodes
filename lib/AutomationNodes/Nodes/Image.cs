using AutomationNodes.Core;
using System.Collections.Generic;

namespace AutomationNodes.Nodes
{
    public class Image : Node
    {
        private readonly INodeOrchestrator nodeOrchestrator;

        public Image(INodeOrchestrator nodeOrchestrator)
        {
            this.nodeOrchestrator = nodeOrchestrator;
        }

        public override string Type => "Img";

        public string ImageName { get; set; }

        public override void OnCreate(object[] parameters)
        {
            base.OnCreate(parameters);

            if (parameters.Length > 0) {
                ImageName = (string)parameters[0];
            }
        }

        public override void OnCreated(Clients clients, object[] parameters)
        {
            base.OnCreated(clients, parameters);
            var nodes = new ClientNode(this);
            nodeOrchestrator.SetProperty(clients, nodes, "position", "absolute");
            if (parameters.Length > 1) {
                nodeOrchestrator.SetProperty(clients, nodes, "width", (string)parameters[1]);
            }
            if (parameters.Length > 2) {
                nodeOrchestrator.SetProperty(clients, nodes, "height", (string)parameters[2]);
            }
        }

        public override Dictionary<string, object> CreationMessage()
        {
            var message = base.CreationMessage();
            message.Add("imageName", ImageName);
            return message;
        }
    }
}
