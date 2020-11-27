using AutomationNodes.Core;
using System.Collections.Generic;

namespace AutomationNodes.Nodes
{
    public class Image : Node
    {
        private readonly INodeCommander nodeCommander;

        public Image(INodeCommander nodeCommander)
        {
            this.nodeCommander = nodeCommander;
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

        public override void OnCreated(object[] parameters)
        {
            base.OnCreated(parameters);
            nodeCommander.SetProperty(this, "position", "absolute");
            if (parameters.Length > 1) {
                nodeCommander.SetProperty(this, "width", (string)parameters[1]);
            }
            if (parameters.Length > 2) {
                nodeCommander.SetProperty(this, "height", (string)parameters[2]);
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
