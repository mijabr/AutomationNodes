using AutomationNodes.Core;
using System.Collections.Generic;

namespace AutomationNodes.Nodes
{
    public class Image : Node
    {
        public override string Type => "Img";

        public string ImageName { get; set; }

        public override void OnCreate(object[] parameters)
        {
            base.OnCreate(parameters);

            if (parameters.Length > 0)
            {
                ImageName = (string)parameters[0];
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
