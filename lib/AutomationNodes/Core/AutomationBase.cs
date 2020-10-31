using System;
using System.Collections.Generic;
using System.Linq;
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

        public double DirectionTo(Point location)
        {
            var dx = location.X - X;
            var dy = location.Y - Y;
            double radians;
            if (dy > 0)
            {
                radians = Math.Atan(dx / dy) - Math.PI;
            }
            else if (dy < 0)
            {
                if (dx >= 0)
                {
                    radians = Math.Atan(dx / dy);
                }
                else
                {
                    radians = Math.Atan(dx / dy) - Math.PI * 2;
                }
            }
            else
            {
                radians = dx > 0 ? -Math.PI / 2 : -Math.PI / 2 * 3;
            }

            return -(180 / Math.PI) * radians;
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }

    public abstract class AutomationBase : ITemporalEventHandler
    {
        protected readonly WorldCatalogue worldCatalogue;
        protected readonly WorldBase world;

        public AutomationBase(WorldCatalogue worldCatalogue, WorldBase world)
        {
            this.worldCatalogue = worldCatalogue;
            this.world = world;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string Image { get; set; }
        public abstract string Type { get; }
        public Point Location { get; set; } = new Point();
        public Point Heading { get; private set; } = new Point();
        public TimeSpan HeadingEta { get; private set; }
        public double Speed { get; set; } = 15;
        public double Rotation { get; set; }
        public string InnerHtml { get; set; }

        public AutomationBase Parent { get; set; }

        public void SetLocation(Point location)
        {
            Location = location;
            Heading = location;
            HeadingEta = TimeSpan.Zero;
            world.MoveNode(this);
        }

        public void MoveTo(Point location, TimeSpan? time = null)
        {
            Heading = location;
            HeadingEta = time ?? TimeSpan.FromMilliseconds(Location.DistanceTo(Heading) / Speed * 1000);
            world.MoveNode(this);
        }

        public T CreateNode<T>() where T : AutomationBase
        {
            return world.CreateChildNode<T>(this);
        }

        public virtual void OnCreated()
        {
        }

        public virtual void OnEvent(TemporalEvent t)
        {
        }

        public virtual AutomationMessage CreateMessage() => new AutomationMessage
        {
            Message = "Create",
            Id = Id,
            Type = Type,
            ParentId = Parent?.Id
        };

        public virtual AutomationMessage MoveMessage() => new AutomationMessage
        {
            Message = "Move",
            Id = Id,
            Location = Location,
            Heading = Heading,
            HeadingEta = HeadingEta.TotalMilliseconds
        };

        public virtual AutomationMessage RotateMessage() => new AutomationMessage
        {
            Message = "Rotate",
            Id = Id,
            Rotation = Rotation
        };
    }

    public class AutomationMessage
    {
        public string Message { get; set; }
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string Image { get; set; }
        public string Type { get; set; }
        public Point Location { get; set; }
        public Point Heading { get; set; }
        public double HeadingEta { get; set; }
        public double Rotation { get; set; }
        public string InnerHtml { get; set; }
        public IEnumerable<AutomationMessage> Children { get; set; }
    }
}
