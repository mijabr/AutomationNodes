using AutomationNodes.Core.Compile;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace AutomationNodesTests
{
    public class ScriptTokenizerTests
    {
        private class TestState
        {
            public ScriptTokenizer ScriptTokenizer { get; }
            public string Script { get; set; }

            public TestState()
            {
                ScriptTokenizer = new ScriptTokenizer();
            }

            public List<string> WhenITokenize(TokenParameters separators = null)
            {
                var tokenContext = ScriptTokenizer.Tokenize(Script);
                var tokens = new List<string>();
                var token = tokenContext.NextToken(separators);
                while (token != null) {
                    tokens.Add(token);
                    token = tokenContext.NextToken(separators);
                }

                return tokens;
            }
        }

        [Test]
        public void Tokenize_ShouldSplitTokens()
        {
            var state = new TestState();
            state.Script = "  abc def ghi ";

            var tokens = state.WhenITokenize();

            tokens.Should().BeEquivalentTo(new[] { "  abc def ghi " });
        }

        [Test]
        public void Tokenize_ShouldSplitTokensByWhitespace()
        {
            var state = new TestState();
            state.Script = "  abc def ghi ";

            var tokens = state.WhenITokenize(new TokenParameters { SplitByWhitespace = true } );

            tokens.Should().BeEquivalentTo(new[] { "abc", "def", "ghi" });
        }

        [Test]
        public void Tokenize_ShouldSplitTokensBySeparator_GivenSeparators()
        {
            var state = new TestState();
            state.Script = "abc,123,888";

            var tokens = state.WhenITokenize(new TokenParameters { Separators = new char[] { ',' } });

            tokens.Should().BeEquivalentTo(new[] { "abc", ",", "123", ",", "888" });
        }

        [Test]
        public void Tokenize_ShouldSplitTokensWithTokenGroups_GivenTokenGroups()
        {
            var state = new TestState();
            state.Script = "([position:absolute,transform:rotate(90.5deg)])";

            var tokenGroups = new List<TokenGroup> { new TokenGroup('(', ')') };
            var tokens = state.WhenITokenize(new TokenParameters { Separators = new char[] { ',', ':', '(', '[', ']', ')' }, TokenGroups = tokenGroups });

            tokens.Should().BeEquivalentTo(new[] { "(", "[", "position", ":", "absolute", ",", "transform", ":", "rotate(90.5deg)", "]", ")" });
        }

        [Test]
        public void Tokenize_ShouldSplitTokensBySeparatorString_GivenSeparatorString()
        {
            var state = new TestState();
            state.Script = @" //comment
";

            var tokens = state.WhenITokenize(new TokenParameters { 
                TokenStrings = new string[] { "//" }, Separators = new char[] { '\r' },
                SplitByWhitespace = true
            });

            tokens.Should().BeEquivalentTo(new[] { "//", "comment", "\r" });
        }
    }
}
