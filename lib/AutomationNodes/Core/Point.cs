using System;

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
}
