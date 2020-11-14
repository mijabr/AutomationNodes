using AutomationNodes.Core;
using System.Collections.Generic;

namespace AutomationNodes.Nodes
{
    public class Image : AutomationBase
    {
        public Image(WorldBase world, string imageName) : base(world)
        {
            ImageName = imageName;
        }

        public override string Type => "Img";
        public string ImageName { get; set; }

        public override Dictionary<string, object> CreationMessage()
        {
            var message = base.CreationMessage();
            message.Add("imageName", ImageName);
            return message;
        }
    }
}
