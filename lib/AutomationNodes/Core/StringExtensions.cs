using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AutomationNodes.Core
{
    public static class StringExtensions
    {
        public static string[] SplitAndTrimEx(this string str, params char[] sepatator)
        {
            var strings = new List<string>();
            var current = new StringBuilder();
            var depth = 0;
            foreach (var c in str)
            {
                if (c == '(') depth++;
                if (c == ')') depth--;
                if (depth == 0 && sepatator.Contains(c))
                {
                    strings.Add(current.ToString().Trim());
                    current = new StringBuilder();
                }
                else
                {
                    current.Append(c);
                }
            }

            strings.Add(current.ToString().Trim());

            return strings.ToArray();
        }

        public static string[] GetCommandAndQuotedAndTrim(this string str, char startChar, char endChar)
        {
            var before = new StringBuilder();
            var quoted = new StringBuilder();
            var depth = 0;
            foreach (var c in str)
            {
                if (c == startChar)
                {
                    if (depth++ == 0) continue;
                }
                if (c == endChar)
                {
                    if (--depth == 0) continue;
                }
                if (depth == 0)
                {
                    before.Append(c);
                }
                else
                {
                    quoted.Append(c);
                }
            }

            return new[] { before.ToString().Trim(), quoted.ToString().Trim() }.ToArray();

        }

        public static string[] SplitAndTrim(this string str, params char[] sepatator)
        {

            return str.Split(sepatator).Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
        }
    }
}
