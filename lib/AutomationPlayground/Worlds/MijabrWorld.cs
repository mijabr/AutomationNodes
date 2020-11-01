using AutomationNodes.Core;
using AutomationPlayground.Nodes;
using System;

namespace AutomationPlayground.Worlds
{
    public class MijabrWorld : WorldBase
    {
        public MijabrWorld(WorldCatalogue worldCatalogue, WorldTime worldTime, string connectionId, IHubManager hubManager) : base(worldCatalogue, worldTime, connectionId, hubManager)
        {
        }

        public override void OnCreated()
        {
            base.OnCreated();

            SetProperty("width", "1000px");
            SetProperty("height", "1000px");
            SetProperty("color", "white");
            SetProperty("background-color", "black");
            SetProperty("overflow", "hidden");

            var m = CreateNode<TextNode>("M");
            var i = CreateNode<TextNode>("I");
            var j = CreateNode<TextNode>("J");
            var a = CreateNode<TextNode>("A");
            var b = CreateNode<TextNode>("B");
            var r = CreateNode<TextNode>("R");
            m.SetProperty("position", "absolute");
            i.SetProperty("position", "absolute");
            j.SetProperty("position", "absolute");
            a.SetProperty("position", "absolute");
            b.SetProperty("position", "absolute");
            r.SetProperty("position", "absolute");

            m.SetProperty("left", "100px");
            m.SetProperty("top", "100px");
            i.SetProperty("left", "200px");
            i.SetProperty("top", "300px");
            j.SetProperty("left", "300px");
            j.SetProperty("top", "600px");
            a.SetProperty("left", "400px");
            a.SetProperty("top", "1000px");
            b.SetProperty("left", "500px");
            b.SetProperty("top", "800px");
            r.SetProperty("left", "600px");
            r.SetProperty("top", "200px");


            var time = TimeSpan.FromSeconds(1);
            TransitionTo(m, 0, 0, time);
            TransitionTo(i, 15, 0, time);
            TransitionTo(j, 22, 0, time);
            TransitionTo(a, 28, 0, time);
            TransitionTo(b, 40, 0, time);
            TransitionTo(r, 52, 0, time);
        }

        private void TransitionTo(TextNode node, double x, double y, TimeSpan time)
        {
            node.SetTransition(new System.Collections.Generic.Dictionary<string, string>
            {
                { "left", $"{x}px" },
                { "top", $"{y}px" }
            }, time);
        }
    }
}
