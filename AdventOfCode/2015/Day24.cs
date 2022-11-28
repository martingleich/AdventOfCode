using ProblemsLibrary;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AdventOfCode._2015
{
    [Problem("2015-24-01", MethodName = nameof(ExecutePart1))]
    [Problem("2015-24-02", MethodName = nameof(ExecutePart2))]
    public class Day24
    {
        private static IEnumerable<(ImmutableList<int>, ImmutableArray<int>)> PickElementsThatSumTo(int[] values, int targetSum)
        {
            var elements = new[] { (0, ImmutableList<int>.Empty, 0) }.ToList();
            while (elements.Count > 0)
            {
                var next = new List<(int, ImmutableList<int>, int)>();
                foreach (var elem in AddElements(values, elements, targetSum))
                {
                    if (elem.Item1 == targetSum)
                        yield return (elem.Item2, values.Except(elem.Item2).ToImmutableArray());
                    else
                        next.Add(elem);
                }
                elements = next;
            }
        }
        private static IEnumerable<(int, ImmutableList<int>, int)> AddElements(
            int[] values,
            IEnumerable<(int, ImmutableList<int>, int)> baseCases,
            int targetSum)
        {
            foreach (var b in baseCases)
            {
                var (partial_sum, picked, min_index) = b;
                for(int i = min_index; i < values.Length; ++i)
                {
                    int x = values[i];
                    if (partial_sum + x <= targetSum)
                        yield return (partial_sum + x, picked.Add(x), i + 1);
                }
            }
        }

        private static bool CanSplitIntoEqualSumSets(ImmutableArray<int> values, int targetSum, int count)
        {
            if (values.Sum() != targetSum * count)
                return false;

            return CanSplit(values, 0, new int[count], targetSum);
            static bool CanSplit(ImmutableArray<int> values, int n, int[] subSums, int targetSum)
            {
                if (n == values.Length)
                {
                    return subSums.All(x => x == targetSum);
                }
                for(int i = 0; i < subSums.Length; ++i)
                {
                    subSums[i] += values[n];
                    if (subSums[i] <= targetSum && CanSplit(values, n + 1, subSums, targetSum))
                        return true;
                    subSums[i] -= values[n];
                }
                return false;
            }
        }
        private static ulong Execute(string input, int parts)
        {
            var values = input.SplitLines().Select(int.Parse).OrderByDescending(x => x).ToArray();
            var upper_bound = values.Sum() / parts;
            int min_first_size = int.MaxValue;
            ulong min_quantum_entanglement = ulong.MaxValue;
            foreach(var x in PickElementsThatSumTo(values, upper_bound))
            {
                if (x.Item1.Count > min_first_size)
                    break;
                if (CanSplitIntoEqualSumSets(x.Item2, upper_bound, parts - 1))
                {
                    min_first_size = x.Item1.Count;
                    min_quantum_entanglement = Math.Min(min_quantum_entanglement, x.Item1.Aggregate(1ul, (x, y) => checked(x * (ulong)y)));
                }
            }
            return min_quantum_entanglement;
        }
        public static ulong ExecutePart1(string input) => Execute(input, 3);
        public static ulong ExecutePart2(string input) => Execute(input, 4);
    }
}
