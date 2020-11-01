using AutomationNodes.Core;
using AutomationNodes.Nodes;
using System.Collections.Generic;

namespace AutomationPlayground.Nodes
{
    public class TextNode : DivNode
    {
        public TextNode(WorldCatalogue worldCatalogue, WorldBase world, string text) : base(worldCatalogue, world)
        {
            InnerHtml = text;
        }
        public string InnerHtml { get; set; }

        public override Dictionary<string, object> CreationMessage()
        {
            var message = base.CreationMessage();
            message.Add("innerHtml", InnerHtml);
            return message;
        }
    }
}
