using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomationNodes.Core.Compile
{
    public interface IScriptTokenizer
    {
        ITokenContext Tokenize(string script);
    }

    public interface ITokenContext
    {
        string NextToken(TokenParameters separators);
    }

    public class TokenGroup
    {
        public TokenGroup(char opener, char closer)
        {
            Opener = opener;
            Closer = closer;
        }

        public char Opener { get; }
        public char Closer { get; }
    }

    public class TokenParameters
    {
        public IEnumerable<TokenGroup> TokenGroups { get; set; }
        public char[] Separators { get; set; }
        public string[] TokenStrings { get; set; }
        public bool SplitByWhitespace { get; set; }
    }

    public class ScriptTokenizer : IScriptTokenizer
    {
        public ITokenContext Tokenize(string script)
        {
            return new TokenContext(script);
        }

        private class TokenContext : ITokenContext
        {
            public TokenContext(string script)
            {
                this.script = script;
                position = 0;
            }

            private string script;
            private int position;

            private enum TokenTypes
            {
                Any,
                ConstructorParameter,
                SetFunctionParameter
            }

            public string NextToken(TokenParameters tokenParameters)
            {
                var token = new StringBuilder();
                int[] tokenGroupCounts = new int[tokenParameters?.TokenGroups?.Count() ?? 0];
                int[] tokenStringMatches = new int[tokenParameters?.TokenStrings?.Count() ?? 0];
                while (position < script.Length) {
                    char c = script[position];
                    position++;
                    if ((tokenParameters?.SplitByWhitespace ?? false) && char.IsWhiteSpace(c) && (!tokenParameters?.Separators?.Contains(c) ?? true)) {
                        if (token.Length > 0) {
                            return token.ToString();
                        }
                    } else {
                        if (tokenParameters?.TokenStrings?.Matches(tokenStringMatches, c) ?? false) {
                            token.Append(c);
                            return token.ToString();
                        } else if (tokenParameters?.TokenGroups?.Contains(tokenGroupCounts, c) ?? false) {
                            token.Append(c);
                        } else if (tokenParameters?.Separators?.Contains(c) ?? false) {
                            if (token.Length > 0) {
                                position--;
                                return token.ToString();
                            }
                            return $"{c}";
                        } else {
                            token.Append(c);
                        }
                    }
                }

                if (token.Length > 0) {
                    return token.ToString();
                }

                return null;
            }
        }
    }

    public static class CharExtensions
    {
        public static bool Contains(this char[] chars, char c)
        {
            return chars.Any(ch => ch == c);
        }

        public static bool Contains(this IEnumerable<TokenGroup> tokenGroups, int[] tokenGroupCounts, char c)
        {
            var index = 0;
            foreach (var tokenGroup in tokenGroups) {
                if (c == tokenGroup.Opener) {
                    tokenGroupCounts[index]++;
                    return true;
                } else if (c == tokenGroup.Closer) {
                    if (tokenGroupCounts[index] == 0) {
                        return false;
                    }

                    tokenGroupCounts[index]--;
                    return true;
                }

                index++;
            }

            return false;
        }

        public static bool Matches(this IEnumerable<string> tokenStrings, int[] tokenStringMatches, char c)
        {
            var index = 0;
            foreach (var tokenString in tokenStrings) {
                if (c == tokenString[tokenStringMatches[index]]) {
                    tokenStringMatches[index]++;
                    if (tokenStringMatches[index] == tokenString.Length) {
                        return true;
                    }
                } else {
                    tokenStringMatches[index] = 0;
                }
            }

            return false;
        }
    }
}
