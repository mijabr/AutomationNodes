using AutomationNodes.Core;
using AutomationNodes.Nodes;

namespace AutomationPlayground.Nodes
{
    public class SpeechBubble : Div
    {
        private readonly string text;

        public SpeechBubble(WorldBase world, string text) : base(world)
        {
            this.text = text;
        }

        public override void OnCreated()
        {
            base.OnCreated();

            var bubble = world.CreateChildNode<Text>(this, text);
            bubble.SetProperty("background", "white");
            bubble.SetProperty("color", "black");
            bubble.SetProperty("border-radius", "4em");
            bubble.SetProperty("position", "absolute");
            bubble.SetProperty("padding", "10px");
        }
//        .speech-bubble {
//    position: relative;
//    background: #00aabb;
//    border-radius: .4em;
//}

//.speech-bubble:after {
//    content: '';
//    position: absolute;
//    right: 0;
//    top: 50%;
//    width: 0;
//    height: 0;
//    border: 41px solid transparent;
//    border-left-color: #00aabb;
//    border-right: 0;
//    border-bottom: 0;
//    margin-top: -20.5px;
//    margin-right: -41px;
    }
}
