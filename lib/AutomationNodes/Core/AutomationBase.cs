using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutomationNodes.Core
{
    public struct Point
    {
        public Point(double x = 0, double y = 0)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }

        public double DistanceTo(Point location)
        {
            return Math.Sqrt((location.X - X) * (location.X - X) + (location.Y - Y) * (location.Y - Y));
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }

    public abstract class AutomationBase : ITemporalEventHandler
    {
        protected readonly WorldCatalogue worldCatalogue;
        protected readonly World world;

        public AutomationBase(WorldCatalogue worldCatalogue, World world)
        {
            this.worldCatalogue = worldCatalogue;
            this.world = world;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public abstract string Image { get; }
        public Point Location { get; set; } = new Point();
        public Point Heading { get; private set; } = new Point();
        public double HeadingEta { get; private set; }
        public double Speed { get; set; } = 15;
        public AutomationBase Parent { get; set; }
        public IEnumerable<AutomationBase> Children { get; set; }

        public void MoveTo(Point location)
        {
            Task.Run(() => MoveToAsync(location));
        }

        public async Task MoveToAsync(Point location)
        {
            Heading = location;
            HeadingEta = Location.DistanceTo(Heading) / Speed * 1000;
            await worldCatalogue.MoveNode(world, this);
        }

        public virtual void OnCreated()
        {
        }

        public virtual void OnEvent(TemporalEvent t)
        {
        }
    }
}
