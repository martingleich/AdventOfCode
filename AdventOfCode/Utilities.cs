using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode
{
	public static class Utilities
	{
		public static IEnumerable<string> SplitLines(this string self, bool ignoreEmpty)
		{
			var result = Regex.Split(self, "\r\n|\r|\n").Select(s => s.Trim());
			if (ignoreEmpty)
				return result.Where(l => l.Length != 0);
			return result;
		}
		public static IEnumerable<string> SplitLines(this string self) => self.SplitLines(true);
		public static int Min(params int[] values) => values.Min();
		public static IEnumerable<T> WhereStructNotNull<T>(this IEnumerable<T?> self) where T : struct
		{
			foreach (var maybeX in self)
				if (maybeX is T x)
					yield return x;
		}
		public static T[,] ToSquareArray<T>(this IEnumerable<T> values)
		{
			var elems = values.ToArray();
			var approxSquare = (int)Math.Sqrt(elems.Length);
			if (approxSquare * approxSquare != elems.Length)
				throw new ArgumentException($"Length of enumerable must be a square number");
			var result = new T[approxSquare, approxSquare];
			int r = 0;
			int c = 0;
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
		public static IEnumerable<TAggregate> RunningAggregate<TSource, TAggregate>(this IEnumerable<TSource> values, TAggregate seed, Func<TAggregate, TSource, TAggregate> func)
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
			{
				yield return items;
			}
			else
			{
				for (int i = 0; i < items.Length; ++i)
				{
					var remainingItems = items.RemoveAt(i);
					foreach (var permutation in EachPermutation(remainingItems))
					{
						yield return permutation.Prepend(items[i]).ToImmutableArray();
					}
				}
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
				{
					yield return permutation.Prepend(items[0]).ToImmutableArray();
				}
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
				for (int i = 0; i <= total; ++i)
				{
					foreach (var rest in SplitTotal(total - i, parts - 1))
						yield return rest.Insert(0, i);
				}
			}
		}

		public static IEnumerable<(T1, T2)> ZipTuple<T1, T2>(IEnumerable<T1> a, IEnumerable<T2> b) => a.Zip(b, (a, b) => (a, b));

		public static bool IsSubset<TKey, TValue>(ImmutableDictionary<TKey, TValue> a, ImmutableDictionary<TKey, TValue> subset) where TKey : notnull
			=> subset.All(x => a.TryGetValue(x.Key, out var aValue) && EqualityComparer<TValue>.Default.Equals(aValue, x.Value));
	}
}
