using System;

namespace AutomationNodes.Core.Compile
{
    public static class StringExtension
    {
        public static TimeSpan ToTimeSpan(this string s)
        {
            if (!int.TryParse(s, out int ms)) {
                throw new Exception($"Expected time span but got {s}");
            }

            return TimeSpan.FromMilliseconds(ms);
        }

        public static bool IsKeyword(this string token, string keyword)
        {
            return string.Equals(keyword, token, StringComparison.InvariantCulture);
        }
    }
}
