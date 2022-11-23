using ProblemsLibrary;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AventOfCode._2015
{
	[Problem("2015-16")]
	public class Day16
	{
		public class Sue
		{
			public readonly int Id;
			public readonly ImmutableDictionary<string, int> Things;

			public Sue(int id, ImmutableDictionary<string, int> things)
			{
				Id = id;
				Things = things;
			}

			public static Sue FromString(string line)
			{
				// Sue 1: children: 1, cars: 8, vizslas: 7
				int first = line.IndexOf(':');
				var name = line.Substring(0, first);
				var rest = line[(first + 1)..];
				int id = int.Parse(name.Remove(0, "Sue ".Length));
				var builder = ImmutableDictionary.CreateBuilder<string, int>();
				foreach (var e in rest.Split(",", StringSplitOptions.RemoveEmptyEntries))
				{
					var v = e.Split(":");
					builder.Add(v[0].Trim(), int.Parse(v[1]));
				}

				return new Sue(id, builder.ToImmutable());
			}
		}
		
		public static readonly ImmutableDictionary<string,int> Facts = new Dictionary<string, int>
		{
			["children"] = 3,
			["cats"] = 7,
			["samoyeds"] = 2,
			["pomeranians"] = 3,
			["akitas"] = 0,
			["vizslas"] = 0,
			["goldfish"] = 5,
			["trees"] = 3,
			["cars"] = 2,
			["perfumes"] = 1,
		}.ToImmutableDictionary();
		public int Execute(string input)
		{
			var sues = LoadSues(input);
			return sues.Single(s => Utilities.IsSubset(Facts, s.Things)).Id;
		}

		public static ImmutableArray<Sue> LoadSues(string input) => input.SplitLines().Select(Sue.FromString).ToImmutableArray();

	}
	
	[Problem("2015-16-2")]
	public class Day16Part2
	{
		public int Execute(string input)
		{
			var sues = Day16.LoadSues(input);
			return sues.Single(s => Matches(s, Day16.Facts)).Id;
		}

		public bool Matches(Day16.Sue sue, ImmutableDictionary<string, int> facts)
		{
			foreach (var f in facts)
			{
				if (sue.Things.TryGetValue(f.Key, out var sueValue))
				{
					if (f.Key == "cats" || f.Key == "trees")
					{
						if (sueValue <= f.Value)
							return false;
					}
					else if (f.Key == "pomeranians" || f.Key == "goldfish")
					{
						if (sueValue >= f.Value)
							return false;
					}
					else
					{
						if (sueValue != f.Value)
							return false;
					}
				}
			}
			return true;
		}
	}
}
