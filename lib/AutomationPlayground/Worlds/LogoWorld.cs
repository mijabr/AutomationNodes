using AutomationNodes.Core;
using AutomationPlayground.Nodes;
using System;

namespace AutomationPlayground.Worlds
{
    public class LogoWorld : WorldBase
    {
        public LogoWorld(WorldCatalogue worldCatalogue, WorldTime worldTime, string connectionId, IHubManager hubManager) : base(worldCatalogue, worldTime, connectionId, hubManager)
        {
        }

        public override void OnCreated()
        {
            base.OnCreated();

            var m = CreateNode<TextNode>("M");
            var i = CreateNode<TextNode>("I");
            var j = CreateNode<TextNode>("J");
            var a = CreateNode<TextNode>("A");
            var b = CreateNode<TextNode>("B");
            var r = CreateNode<TextNode>("R");
            m.SetLocation(new Point(100, 100));
            i.SetLocation(new Point(200, 300));
            j.SetLocation(new Point(300, 600));
            a.SetLocation(new Point(400, 1000));
            b.SetLocation(new Point(500, 800));
            r.SetLocation(new Point(600, 200));
            var time = TimeSpan.FromSeconds(1);
            m.MoveTo(new Point(0, 0), time);
            i.MoveTo(new Point(15, 0), time);
            j.MoveTo(new Point(22, 0), time);
            a.MoveTo(new Point(28, 0), time);
            b.MoveTo(new Point(40, 0), time);
            r.MoveTo(new Point(52, 0), time);
        }
    }
}
