using AutomationNodes.Core;

namespace AutomationNodes.Nodes
{
    public class ImageNode : AutomationBase
    {
        public ImageNode(WorldCatalogue worldCatalogue, WorldBase world, string image) : base(worldCatalogue, world)
        {
            Image = image;
        }

        public override string Type => "Img";

        public override AutomationMessage CreateMessage()
        {
            var message = base.CreateMessage();
            message.Image = Image;
            return message;
        }
    }
}
