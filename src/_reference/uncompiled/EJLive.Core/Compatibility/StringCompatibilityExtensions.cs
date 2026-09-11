using System;

namespace System
{
    internal static class StringCompatibilityExtensions
    {
        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.IndexOf(value, comparisonType) >= 0;
        }
    }
}
