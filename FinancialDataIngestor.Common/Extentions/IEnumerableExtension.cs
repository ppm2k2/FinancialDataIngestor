using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundAdminRestAPI.Common.Extentions
{
    [ExcludeFromCodeCoverage]
    public static class IEnumerableExtension
    {
        public static bool HasItems<T>(this IEnumerable<T> items)
        {
            if (items != null && items.Any())
                return true;
            return false;
        }
        public static bool IsEmpty<T>(this IEnumerable<T> items)
        {
            if (items == null || !items.Any())
                return true;
            return false;
        }
        public static bool SafeAny<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if(items.HasItems())
            {
                return items.Any(predicate);
            }
            return false;
        }
    }
}
