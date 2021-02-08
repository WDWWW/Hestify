using System.Collections.Generic;

namespace Hestify.Helpers
{
    internal static class EnumerableHelper
    {
        internal static IEnumerable<T> WithOne<T>(this IEnumerable<T> enumerable, T newOne)
        {
            foreach (var item in enumerable)
            {
                yield return item;
            }
            yield return newOne;
        }
        
    }
}