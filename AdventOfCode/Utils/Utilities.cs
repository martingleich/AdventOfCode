using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AdventOfCode.Utils;

public static class Utilities
{
    [Flags]
    public enum SplitLineOptions
    {
        None = 0,
        IgnoreEmpty = 1,
        Trim = 2
    }

    [Flags]
    public enum SplitOptions
    {
        None = 0,
        IgnoreEmpty = 1
    }

    public static IEnumerable<string> SplitLines(this string self, SplitLineOptions option)
    {
        var options = StringSplitOptions.None;
        if ((option & SplitLineOptions.Trim) != 0)
            options = StringSplitOptions.TrimEntries;
        if ((option & SplitLineOptions.IgnoreEmpty) != 0)
            options = StringSplitOptions.RemoveEmptyEntries;
        var result = self.Split(new[] { "\r\n", "\n", "\r" }, options).AsEnumerable();
        return result;
    }

    public static IEnumerable<string> SplitLines(this string self, bool ignoreEmpty)
    {
        return self.SplitLines(
            SplitLineOptions.Trim | (ignoreEmpty ? SplitLineOptions.IgnoreEmpty : SplitLineOptions.None));
    }

    public static IEnumerable<string> SplitLines(this string self)
    {
        return self.SplitLines(true);
    }

    public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> self, Func<T, bool> split,
        SplitOptions splitOptions = SplitOptions.IgnoreEmpty)
    {
        var group = default(List<T>?);
        foreach (var value in self)
            if (split(value))
            {
                if ((splitOptions & SplitOptions.IgnoreEmpty) == 0 || (group != null && group.Count > 0))
                {
                    yield return group ?? Enumerable.Empty<T>();
                    group = null;
                }
            }
            else
            {
                group ??= new List<T>();
                group.Add(value);
            }

        if ((splitOptions & SplitOptions.IgnoreEmpty) == 0 || (group != null && group.Count > 0))
            yield return group ?? Enumerable.Empty<T>();
    }

    public static int Min(params int[] values)
    {
        return values.Min();
    }

    public static IEnumerable<T> WhereStructNotNull<T>(this IEnumerable<T?> self) where T : struct
    {
        foreach (var maybeX in self)
            if (maybeX is T x)
                yield return x;
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> self) where T : class
    {
        foreach (var maybeX in self)
            if (maybeX is T x)
                yield return x;
    }

    public static IEnumerable<T> WhereOkay<T>(this IEnumerable<Result<T>> self)
    {
        foreach (var maybeX in self)
            if (maybeX.TryGetValue(out var value))
                yield return value;
    }

    public static IEnumerable<T> AllOkay<T>(this IEnumerable<Result<T>> self)
    {
        foreach (var maybeX in self)
            yield return maybeX.Value;
    }

    public static IEnumerable<T> IntersectAll<T>(this IEnumerable<IEnumerable<T>> self)
    {
        HashSet<T>? hashSet = null;
        foreach (var list in self)
            if (hashSet == null)
                hashSet = list.ToHashSet();
            else
                hashSet.IntersectWith(list);
        return hashSet ?? Enumerable.Empty<T>();
    }

    public static IEnumerable<T> ToRectAndTranspose<T>(this IEnumerable<T> self, int width)
    {
        var result = new List<T>[width];
        for (var i = 0; i < width; ++i)
            result[i] = new List<T>();
        var j = 0;
        foreach (var x in self)
        {
            result[j].Add(x);
            j = (j + 1) % width;
        }

        foreach (var col in result)
        foreach (var x in col)
            yield return x;
    }

    public static T[,] ToSquareArray<T>(this IEnumerable<T> values)
    {
        var elems = values.ToArray();
        var approxSquare = (int)Math.Sqrt(elems.Length);
        if (approxSquare * approxSquare != elems.Length)
            throw new ArgumentException("Length of enumerable must be a square number");
        var result = new T[approxSquare, approxSquare];
        var r = 0;
        var c = 0;
        foreach (var x in elems)
        {
            result[r, c] = x;
            ++c;
            if (c == approxSquare)
            {
                c = 0;
                r++;
            }
        }

        return result;
    }

    public static IEnumerable<TAggregate> RunningAggregate<TSource, TAggregate>(this IEnumerable<TSource> values,
        TAggregate seed, Func<TAggregate, TSource, TAggregate> func)
    {
        var cur = seed;
        yield return cur;
        foreach (var v in values)
        {
            cur = func(cur, v);
            yield return cur;
        }
    }

    public static IEnumerable<ImmutableArray<string>> EachPermutation(ImmutableArray<string> items)
    {
        if (items.Length <= 1)
            yield return items;
        else
            for (var i = 0; i < items.Length; ++i)
            {
                var remainingItems = items.RemoveAt(i);
                foreach (var permutation in EachPermutation(remainingItems))
                    yield return permutation.Prepend(items[i]).ToImmutableArray();
            }
    }

    public static IEnumerable<ImmutableArray<string>> EachCircle(ImmutableArray<string> items)
    {
        if (items.Length <= 1)
        {
            yield return items;
        }
        else
        {
            var remainingItems = items.RemoveAt(0);
            foreach (var permutation in EachPermutation(remainingItems))
                yield return permutation.Prepend(items[0]).ToImmutableArray();
        }
    }

    public static IEnumerable<ImmutableArray<int>> SplitTotal(int total, int parts)
    {
        if (parts == 0)
        {
            if (total == 0)
                yield return ImmutableArray.Create<int>();
            else
                throw new InvalidOperationException();
        }
        else if (parts == 1)
        {
            yield return ImmutableArray.Create(total);
        }
        else
        {
            for (var i = 0; i <= total; ++i)
                foreach (var rest in SplitTotal(total - i, parts - 1))
                    yield return rest.Insert(0, i);
        }
    }

    public static IEnumerable<(T1, T2)> ZipTuple<T1, T2>(IEnumerable<T1> a, IEnumerable<T2> b)
    {
        return a.Zip(b, (a, b) => (a, b));
    }

    public static bool IsSubset<TKey, TValue>(ImmutableDictionary<TKey, TValue> a,
        ImmutableDictionary<TKey, TValue> subset) where TKey : notnull
    {
        return subset.All(x =>
            a.TryGetValue(x.Key, out var aValue) && EqualityComparer<TValue>.Default.Equals(aValue, x.Value));
    }

    public static IEnumerable<KeyValuePair<TGroup, int>> AllCombinatorialSums<TGroup>(
        this IEnumerable<IEnumerable<KeyValuePair<TGroup, int>>> groups,
        Func<TGroup, TGroup, TGroup> combine,
        IComparer<int> comparer)
    {
        return groups
            .Select(g => g.OrderBy(i => i.Value, comparer).AsEnumerable())
            .Aggregate(AddGroup);

        IEnumerable<KeyValuePair<TGroup, int>> AddGroup(IEnumerable<KeyValuePair<TGroup, int>> a,
            IEnumerable<KeyValuePair<TGroup, int>> b)
        {
            var queue =
                new PriorityQueue<(ImmutableEnumerator<KeyValuePair<TGroup, int>>,
                    ImmutableEnumerator<KeyValuePair<TGroup, int>>), int>(comparer);
            var set =
                new HashSet<(ImmutableEnumerator<KeyValuePair<TGroup, int>>,
                    ImmutableEnumerator<KeyValuePair<TGroup, int>>)>();
            using var ea = a.GetEnumerator();
            using var eb = b.GetEnumerator();
            var firstA = ImmutableEnumerator.Create(ea)!;
            var firstB = ImmutableEnumerator.Create(eb)!;
            queue.Enqueue((firstA, firstB), firstA.Current.Value + firstB.Current.Value);
            set.Add((firstA, firstB));
            while (queue.Count > 0)
            {
                queue.TryDequeue(out var tup, out var prio);
                yield return KeyValuePair.Create(combine(tup.Item1.Current.Key, tup.Item2.Current.Key), prio);
                int sum;

                var nextA = tup.Item1.MoveNext();
                if (nextA != null)
                {
                    sum = nextA.Current.Value + tup.Item2.Current.Value;
                    if (set.Add((nextA, tup.Item2))) queue.Enqueue((nextA, tup.Item2), sum);
                }

                var nextB = tup.Item2.MoveNext();
                if (nextB != null)
                {
                    sum = tup.Item1.Current.Value + nextB.Current.Value;
                    if (set.Add((tup.Item1, nextB))) queue.Enqueue((tup.Item1, nextB), sum);
                }
            }
        }
    }

    public static ulong PowMod(ulong x, ulong mod, ulong exp)
    {
        if (exp == 0)
            return 1;
        ulong y = 1;
        while (exp > 1)
            if (exp % 2 == 0)
            {
                x = x * x % mod;
                exp /= 2;
            }
            else
            {
                y = x * y % mod;
                x = x * x % mod;
                exp = (exp - 1) / 2;
            }

        return y * x % mod;
    }

    public static T MostHistogram<T>(this IEnumerable<T> self) where T : notnull
    {
        return Histogram(self).MinBy(x => x.Value).Key;
    }

    public static T LeastHistogram<T>(this IEnumerable<T> self) where T : notnull
    {
        return Histogram(self).MaxBy(x => x.Value).Key;
    }

    public static Dictionary<T, int> Histogram<T>(this IEnumerable<T> self) where T : notnull
    {
        var dict = new Dictionary<T, int>();
        foreach (var x in self)
        {
            dict.TryGetValue(x, out var count);
            dict[x] = count + 1;
        }

        return dict;
    }

    public static Func<TArg, TResult> Memoize<TArg, TResult>(Func<TArg, TResult> func) where TArg : notnull
    {
        var dict = new Dictionary<TArg, TResult>();
        return arg =>
        {
            if (!dict.TryGetValue(arg, out var result))
                result = dict[arg] = func(arg);
            return result;
        };
    }

    public sealed class ReverseComparer<T> : IComparer<T>
    {
        private readonly IComparer<T> _comparer;

        private ReverseComparer(IComparer<T> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public static IComparer<T> Default { get; } = new ReverseComparer<T>(Comparer<T>.Default);

        public int Compare(T? x, T? y)
        {
            return _comparer.Compare(y, x);
        }
    }

    private static class ImmutableEnumerator
    {
        public static ImmutableEnumerator<T>? Create<T>(IEnumerator<T> enumerator)
        {
            if (enumerator.MoveNext())
                return new ImmutableEnumerator<T>(enumerator, 0);
            return null;
        }
    }

    private class ImmutableEnumerator<T> : IEquatable<ImmutableEnumerator<T>>
    {
        private readonly IEnumerator<T> _enumerator;
        private (bool done, ImmutableEnumerator<T>?) _runOnce = (false, null);

        public ImmutableEnumerator(IEnumerator<T> enumerator, int index)
        {
            _enumerator = enumerator;
            Index = index;
            Current = _enumerator.Current;
        }

        public int Index { get; }
        public T Current { get; }

        public bool Equals(ImmutableEnumerator<T>? other)
        {
            return other != null && other.Index == Index;
        }

        public ImmutableEnumerator<T>? MoveNext()
        {
            if (!_runOnce.done)
            {
                var successful = _enumerator.MoveNext();
                _runOnce = (true, successful ? new ImmutableEnumerator<T>(_enumerator, Index + 1) : null);
            }

            return _runOnce.Item2;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ImmutableEnumerator<T>);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }
    }
}

public static class MathULong
{
    public static ulong Gcd(ulong a, ulong b)
    {
        while (b != 0)
            (a, b) = (b, a % b);
        return a;
    }
    public static ulong Lcm(ulong a, ulong b)
    {
        var gcd = Gcd(a, b);
        return (a / gcd) * (b / gcd);
    }
    public static ulong Mul(ulong a, ulong b) => checked(a * b);
    public static ulong Add(ulong a, ulong b) => checked(a + b);
}
public static class MathInt
{
    public static int Add(int a, int b) => checked(a + b);
    public static int Mul(int a, int b) => checked(a * b);
}
public static class MathUInt
{
    public static uint Mul(uint a, uint b) => checked(a * b);
    public static uint Add(uint a, uint b) => checked(a + b);
}
public static class MathBool
{
    public static bool Or(bool a, bool b) => a | b;
}