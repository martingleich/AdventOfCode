using AdventOfCode;
using ProblemsLibrary;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TypesafeParser;

namespace AventOfCode._2015
{
    [Problem("2015-09-1")]
	public class Day09
	{
		private static readonly Func<string, ErrorOr<(string, string, int)>> DistanceParser = Pattern.Empty.AlphaNumeric().Fixed(" to ").AlphaNumeric().Fixed(" = ").AlphaNumeric(int.Parse).EndOfInput().Compile();
		public class Distances
		{
			readonly ImmutableArray<int> Root;
			readonly Dictionary<(int, int), int> Map = new Dictionary<(int, int), int>();
			bool inverseLength = false;
			public Distances(string input)
			{
				Dictionary<string, int> idMap = new Dictionary<string, int>();
				int GetId(string name)
				{
					if (idMap.TryGetValue(name, out var id))
						return id;
					else
						return idMap[name] = idMap.Count;
				}
				foreach (var entry in input.SplitLines().Select(line => DistanceParser(line).Value))
				{
					Add(GetId(entry.Item1), GetId(entry.Item2), entry.Item3);
				}
				Root = idMap.Values.ToImmutableArray();
			}
			public void Add(int a, int b, int length)
			{
				Map[(Math.Min(a, b), Math.Max(a, b))] = length;
			}
			public int GetDistance(int a, int b)
			{
				var v = Map[(Math.Min(a, b), Math.Max(a, b))];
				return inverseLength ? -v : v;
			}

			private int GetMinRoute(int start, ImmutableArray<int> unvisisted)
				=> unvisisted.Length == 0
					? 0
					: unvisisted.Min(x => GetDistance(start, x) + GetMinRoute(x, unvisisted.Remove(x)));
			public int GetMinRoute()
				=> Root.Min(x => GetMinRoute(x, Root.Remove(x)));
			public int GetMaxRoute()
			{
				inverseLength = true;
				var result = -GetMinRoute();
				inverseLength = false;
				return result;
			}
		}

		[TestCase(@"
London to Dublin = 464
London to Belfast = 518
Dublin to Belfast = 141", 605)]
		public int Execute(string input)
		{
			var distances = new Distances(input);
			return distances.GetMinRoute();
		}
	}

	[Problem("2015-09-2")]
	public class Day09Part2
	{
		[TestCase(@"
London to Dublin = 464
London to Belfast = 518
Dublin to Belfast = 141", 982)]
		public int Execute(string input)
		{
			var distances = new Day09.Distances(input);
			return distances.GetMaxRoute();
		}
	}
}
