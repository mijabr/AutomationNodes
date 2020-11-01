using AutomationNodes.Core;
using System.Collections.Generic;

namespace AutomationNodes.Nodes
{
    public class ImageNode : AutomationBase
    {
        public ImageNode(WorldCatalogue worldCatalogue, WorldBase world, string image) : base(worldCatalogue, world)
        {
            Image = image;
        }

        public override string Type => "Img";
        public string Image { get; set; }

        public override Dictionary<string, object> CreationMessage()
        {
            var message = base.CreationMessage();
            message.Add("image", Image);
            return message;
        }
    }
}
