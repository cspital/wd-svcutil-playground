using System;
using System.Linq;

namespace UWDiff
{
    public static class StringExtensions
    {
        public static bool EndsWithEither(this string test, params string[] endings)
        {
            return endings.Any(e => test.EndsWith(e));
        }
    }
}
