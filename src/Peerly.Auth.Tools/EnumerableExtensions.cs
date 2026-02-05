using System;
using System.Collections.Generic;
using System.Linq;

namespace Peerly.Auth.Tools;

public static class EnumerableExtensions
{
    public static TResult[] ToArrayBy<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        return source.Select(selector).ToArray();
    }
}
