using AdventOfCode.Utils;
using ProblemsLibrary;
using System.Collections.Immutable;
using System.Linq;

namespace AventOfCode._2015
{
	[Problem("2015-17")]
	public class Day17
	{
		public int GreedySums(int total, ImmutableArray<int> containers, ImmutableArray<int> remaining, int containerCursor)
		{
			if (total == 0)
				return 1;
			int options = 0;
			for (int i = containerCursor; i < containers.Length; ++i)
			{
				if (total >= containers[i] && total - containers[i] <= remaining[i])
				{
					options += GreedySums(total - containers[i], containers, remaining, i + 1);
				}
			}
			return options;
		}

		[TestCase(@"20
15
10
5
5", 25, 4)]
		public int ExecuteBase(string input, int total)
		{
			var containers = input.SplitLines().Select(int.Parse).OrderByDescending(v => v).ToImmutableArray();
			var totalSpace = containers.Sum();
			var remaining = containers.RunningAggregate(0, (a, b) => a + b).Select(x => totalSpace - x).ToImmutableArray();
			var result = GreedySums(total, containers, remaining, 0);
			return result;
		}
		public int Execute(string input) => ExecuteBase(input, 150);
	}


	[Problem("2015-17-2")]
	public class Day17Part2
	{
		public readonly struct MinCount
		{
			public readonly int minValue;
			public readonly int minCount;

			public MinCount(int minValue, int minCount)
			{
				this.minValue = minValue;
				this.minCount = minCount;
			}

			public MinCount Add(int v)
			{
				if (v == minValue)
					return new MinCount(minValue, minCount + 1);
				else if (v < minValue)
					return new MinCount(v, 1);
				else
					return this;
			}
		}
		public MinCount GreedySums(int total, ImmutableArray<int> containers, ImmutableArray<int> remaining, int containerCursor, int depth, MinCount agg)
		{
			if (total == 0)
				return agg.Add(depth);
			for (int i = containerCursor; i < containers.Length; ++i)
			{
				if (total >= containers[i] && total - containers[i] <= remaining[i] && depth < agg.minValue)
				{
					agg = GreedySums(total - containers[i], containers, remaining, i + 1, depth + 1, agg);
				}
			}
			return agg;
		}

		[TestCase(@"20
15
10
5
5", 25, 3)]
		public int ExecuteBase(string input, int total)
		{
			var containers = input.SplitLines().Select(int.Parse).OrderByDescending(v => v).ToImmutableArray();
			var totalSpace = containers.Sum();
			var remaining = containers.RunningAggregate(0, (a, b) => a + b).Select(x => totalSpace - x).ToImmutableArray();
			var result = GreedySums(total, containers, remaining, 0, 0, new MinCount(int.MaxValue, 0));
			return result.minCount;
		}
		public int Execute(string input) => ExecuteBase(input, 150);
	}
}
