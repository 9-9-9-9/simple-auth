using System;

namespace SimpleAuth.Shared.Extensions
{
    public static class ValueExtensions
    {
        public static string Or(this string left, string right)
        {
            return string.IsNullOrEmpty(left) ? right : left;
        }

        public static T Or<T>(this T left, T right) where T : class
        {
            return left ?? right;
        }

        // ReSharper disable once UnusedMember.Global
        public static T OrStruct<T>(this T left, T right) where T : struct
        {
            return left.Equals(default(T)) ? right : left;
        }

        public static string NormalizeInput(this string source)
        {
            return source?.ToLowerInvariant();
        }

        public static bool IsBlank(this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }

        public static bool IsEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static bool EqualsIgnoreCase(this string left, string right)
        {
            return left.Equals(right, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string TrimToNull(this string text)
        {
            if (text == null) return null;
            var trimmed = text.Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }
    }
}