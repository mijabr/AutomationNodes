using AutomationNodes.Core;
using AutomationNodes.Nodes;

namespace AutomationPlayground.Nodes
{
    public class TextNode : DivNode
    {
        public TextNode(WorldCatalogue worldCatalogue, WorldBase world, string text) : base(worldCatalogue, world)
        {
            InnerHtml = text;
        }

        public override AutomationMessage CreateMessage()
        {
            var message = base.CreateMessage();
            message.InnerHtml = InnerHtml;
            return message;
        }

    }
}
