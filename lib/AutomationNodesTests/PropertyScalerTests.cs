using AutomationNodes.Core;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace AutomationNodesTests
{
    public class PropertyScalerTests
    {
        public class TestState
        {
            public PropertyScaler PropertyScaler { get; }

            public TestState()
            {
                var connectedClients = new Mock<IConnectedClients>();
                connectedClients.Setup(c => c.ClientContexts).Returns(new Dictionary<string, ClientContext>
                {
                    { string.Empty, new ClientContext { ImageScaling = 2.0, FontScaling = 1.5 } }
                });
                PropertyScaler = new PropertyScaler(connectedClients.Object);
            }
        }

        [TestCase("4%", "8%")]
        [TestCase(" 4%", "8%")]
        [TestCase("1.25%", "2.5%")]
        public void ScaleImageProperty_ShouldReturnScaledValue_GivenTwoTimesImageScaling(string value, string expectedScaledValue)
        {
            var state = new TestState();

            state.PropertyScaler.ScaleImageProperty(string.Empty, value).Should().Be(expectedScaledValue);
        }

        [TestCase("1em", "1.5em")]
        public void ScaleFontSizeProperty_ShouldReturnScaledValue_GivenOnePointFiveTimesFontScaling(string value, string expectedScaledValue)
        {
            var state = new TestState();

            state.PropertyScaler.ScaleFontSizeProperty(string.Empty, value).Should().Be(expectedScaledValue);
        }
    }
}
