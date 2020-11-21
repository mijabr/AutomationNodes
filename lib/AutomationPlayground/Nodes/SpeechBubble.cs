using AutomationNodes.Core;
using AutomationNodes.Nodes;

namespace AutomationPlayground.Nodes
{
    public class SpeechBubble : Div
    {
        private readonly INodeCommander nodeCommander;

        public SpeechBubble(INodeCommander nodeCommander)
        {
            this.nodeCommander = nodeCommander;
        }

        private string Text { get; set; }

        public override void OnCreated(object[] parameters)
        {
            base.OnCreated(parameters);

            if (parameters.Length > 0)
            {
                Text = (string)parameters[0];
            }

            var bubble = nodeCommander.CreateChildNode<Text>(this, Text);
            nodeCommander.SetProperty(bubble, "background", "white");
            nodeCommander.SetProperty(bubble, "color", "black");
            nodeCommander.SetProperty(bubble, "border-radius", "4em");
            nodeCommander.SetProperty(bubble, "position", "absolute");
            nodeCommander.SetProperty(bubble, "padding", "10px");
        }
    }
}
