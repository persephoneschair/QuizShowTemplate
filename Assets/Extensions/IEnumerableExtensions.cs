using System;
using System.Collections.Generic;
using System.Linq;

public static class IEnumerableExtensions
{
    public static T RandomPick<T>(this List<T> list)
    {
        int lookupIndex = UnityEngine.Random.Range(0, list.Count);
        return list[lookupIndex];
    }

    public static T RandomPick<T>(this IEnumerable<T> enumerable)
    {
        List<T> lookupList = enumerable.ToList();
        int lookupIndex = UnityEngine.Random.Range(0, lookupList.Count);
        return lookupList[lookupIndex];
    }

    public static IEnumerable<T> RandomPick<T>(this IEnumerable<T> enumerable, int maximumCount, bool uniquePick)
    {
        List<T> lookupList = enumerable.ToList();

        if (uniquePick)
        {
            maximumCount = System.Math.Min(maximumCount, lookupList.Count);

            for (int count = 0; count < maximumCount; ++count)
            {
                int lookupIndex = UnityEngine.Random.Range(count, lookupList.Count);
                yield return lookupList[lookupIndex];
                lookupList[lookupIndex] = lookupList[count];
            }
        }
        else
        {
            for (int count = 0; count < maximumCount; ++count)
            {
                int lookupIndex = UnityEngine.Random.Range(0, lookupList.Count);
                yield return lookupList[lookupIndex];
            }
        }
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable)
    {
        List<T> lookupList = enumerable.ToList();

        for (int count = 0; count < lookupList.Count; ++count)
        {
            int lookupIndex = UnityEngine.Random.Range(count, lookupList.Count);
            yield return lookupList[lookupIndex];
            lookupList[lookupIndex] = lookupList[count];
        }
    }

    //Taken from MoreLINQ/MaxBy.cs
    public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
    {
        return source.MaxBy(selector, null);
    }

    /// <summary>
    /// Returns the maximal element of the given sequence, based on
    /// the given projection and the specified comparer for projected values. 
    /// </summary>
    /// <remarks>
    /// If more than one element has the maximal projected value, the first
    /// one encountered will be returned. This operator uses immediate execution, but
    /// only buffers a single result (the current maximal element).
    /// </remarks>
    /// <typeparam name="TSource">Type of the source sequence</typeparam>
    /// <typeparam name="TKey">Type of the projected element</typeparam>
    /// <param name="source">Source sequence</param>
    /// <param name="selector">Selector to use to pick the results to compare</param>
    /// <param name="comparer">Comparer to use to compare projected values</param>
    /// <returns>The maximal element, according to the projection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="selector"/> 
    /// or <paramref name="comparer"/> is null</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>
    public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
    {
        using (var sourceIterator = source.GetEnumerator())
        {
            if (!sourceIterator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
            var max = sourceIterator.Current;
            var maxKey = selector(max);
            while (sourceIterator.MoveNext())
            {
                var candidate = sourceIterator.Current;
                var candidateProjected = selector(candidate);
                if (Comparer<TKey>.Default.Compare(candidateProjected, maxKey) > 0)
                {
                    max = candidate;
                    maxKey = candidateProjected;
                }
            }
            return max;
        }
    }
}
