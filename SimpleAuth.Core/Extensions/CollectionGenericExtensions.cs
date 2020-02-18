using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleAuth.Core.Extensions
{
    public static class CollectionGenericExtensions
    {
        public static bool IsAny<T>(this IEnumerable<T> source)
        {
            return source?.Any() == true;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return !source.IsAny();
        }

        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> source)
        {
            return source ?? new T[0];
        }

        public static IEnumerable<T> DropNull<T>(this IEnumerable<T> source)
        {
            return source?.Where(x => x != null);
        }

        public static IEnumerable<string> DropBlank(this IEnumerable<string> source)
        {
            return source?.Where(x => !x.IsBlank());
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T newElement)
        {
            if (newElement is null)
                throw new ArgumentNullException(nameof(newElement));
            var newList = source.OrEmpty().ToList();
            newList.Add(newElement);
            return newList;
        }
    }
}