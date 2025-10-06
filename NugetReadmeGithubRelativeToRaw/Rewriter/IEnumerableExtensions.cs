using System;
using System.Collections.Generic;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal static class IEnumerableExtensions
    {
        public static IEnumerable<(TResult Result, TSource Source)> SelectWithSourceNotNull<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult?> selector) where TResult : class
        {
            foreach (var item in source)
            {
                var res = selector(item);
                if (res != null)
                {
                    yield return (res, item);
                }
            }
        }

        public static void ProcessSourceResult<TSource, TResult>(this IEnumerable<(TResult Result, TSource Source)> sourceResults, Action<TResult, TSource> processor)
        {
            foreach (var sourceResult in sourceResults)
            {
                processor(sourceResult.Result, sourceResult.Source);
            }
        }

        public static IEnumerable<(TNewResult result, TSource)> SelectWithSourceNotNull<TSource, TLastResult, TNewResult>(
            this IEnumerable<(TLastResult result, TSource item)> sourceWithResult, Func<TLastResult, TNewResult?> selector) where TNewResult : class
        {
            foreach (var srcResult in sourceWithResult)
            {
                var res = selector(srcResult.result);
                if (res != null)
                {
                    yield return (res, srcResult.item);
                }
            }
        }
    }
}
