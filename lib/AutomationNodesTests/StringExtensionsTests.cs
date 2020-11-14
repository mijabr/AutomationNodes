using AutomationNodes.Core;
using FluentAssertions;
using NUnit.Framework;

namespace AutomationNodesTests
{
    public class StringExtensionsTests
    {
        [Test]
        public void CanSplitBracketedString()
        {
            var split = "var x = Image(image.jpg).set({})".SplitAndTrimEx('.');
            split[0].Should().Be("var x = Image(image.jpg)");
            split[1].Should().Be("set({})");
        }
        [Test]
        public void CanSplitCommandAndQuoted()
        {
            var split = "set({width:300px,transform:rotate(90deg)})".GetCommandAndQuotedAndTrim('(', ')');
            split[0].Should().Be("set");
            split[1].Should().Be("{width:300px,transform:rotate(90deg)}");
        }
    }
}
