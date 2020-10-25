using AutomationNodes.Core;
using FluentAssertions;
using NUnit.Framework;

namespace AutomationNodesTests
{
    public class PointTests
    {
        [Test]
        public void DistanceTo_ShouldReturnDistance_GivenTwoPoints()
        {
            new Point(0, 0).DistanceTo(new Point(3, 4)).Should().Be(5);
        }

        [Test]
        public void DirectionTo_ShouldReturnAngle0_GivenTwoPoints()
        {
            new Point(0, 0).DirectionTo(new Point(0, -10)).Should().Be(0);
        }

        [Test]
        public void DirectionTo_ShouldReturnAngle26_GivenTwoPoints()
        {
            new Point(0, 0).DirectionTo(new Point(5, -10)).Should().BeApproximately(26.5, 0.1);
        }

        [Test]
        public void DirectionTo_ShouldReturnAngle45_GivenTwoPoints()
        {
            new Point(0, 0).DirectionTo(new Point(10, -10)).Should().Be(45);
        }

        [Test]
        public void DirectionTo_ShouldReturnAngle90_GivenTwoPoints()
        {
            new Point(0, 0).DirectionTo(new Point(10, 0)).Should().Be(90);
        }

        [Test]
        public void DirectionTo_ShouldReturnAngle116_GivenTwoPoints()
        {
            new Point(0, 0).DirectionTo(new Point(10, 5)).Should().BeApproximately(116.5, 0.1);
        }

        [Test]
        public void DirectionTo_ShouldReturnAngle135_GivenTwoPoints()
        {
            new Point(0, 0).DirectionTo(new Point(10, 10)).Should().Be(135);
        }

        [Test]
        public void DirectionTo_ShouldReturnAngle180_GivenTwoPoints()
        {
            new Point(0, 0).DirectionTo(new Point(0, 10)).Should().Be(180);
        }

        [Test]
        public void DirectionTo_ShouldReturnAngle206_GivenTwoPoints()
        {
            new Point(0, 0).DirectionTo(new Point(-5, 10)).Should().BeApproximately(206.5, 0.1);
        }

        [Test]
        public void DirectionTo_ShouldReturnAngle225_GivenTwoPoints()
        {
            new Point(0, 0).DirectionTo(new Point(-10, 10)).Should().Be(225);
        }

        [Test]
        public void DirectionTo_ShouldReturnAngle270_GivenTwoPoints()
        {
            new Point(0, 0).DirectionTo(new Point(-10, 0)).Should().Be(270);
        }

        [Test]
        public void DirectionTo_ShouldReturnAngle296_GivenTwoPoints()
        {
            new Point(0, 0).DirectionTo(new Point(-10, -5)).Should().BeApproximately(296.5, 0.1);
        }

        [Test]
        public void DirectionTo_ShouldReturnAngle315_GivenTwoPoints()
        {
            new Point(0, 0).DirectionTo(new Point(-10, -10)).Should().Be(315);
        }
    }
}
