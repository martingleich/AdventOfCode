using ProblemsLibrary;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TypesafeParser;

namespace AventOfCode._2015
{
	[Problem("2015-13")]
	public class Day13
	{
		public static bool MatchSign(string value) => value switch
		{
			"lose" => false,
			"gain" => true,
			_ => throw new ArgumentException(nameof(value))
		};
		public static readonly Func<string, ErrorOr<(string First, bool Sign, int Mag, string Second)>> LineParser = Pattern.Empty.
			AlphaNumeric().
			Fixed(" would ").
			AlphaNumeric(MatchSign).
			Fixed(" ").
			AlphaNumeric(int.Parse).
			Fixed(" happiness units by sitting next to ").
			AlphaNumeric().
			Fixed(".").
			EndOfInput().Compile();


		public static int EvalCircle(ImmutableArray<string> chars, Dictionary<(string, string), int> info)
		{
			int total = 0;
			for (int i = 0; i < chars.Length; ++i)
			{
				var left = chars[(i + chars.Length - 1) % chars.Length];
				var self = chars[i];
				var right = chars[(i + 1) % chars.Length];
				total += info[(self, left)];
				total += info[(self, right)];
			}
			return total;
		}

		[TestCase(@"
Alice would gain 54 happiness units by sitting next to Bob.
Alice would lose 79 happiness units by sitting next to Carol.
Alice would lose 2 happiness units by sitting next to David.
Bob would gain 83 happiness units by sitting next to Alice.
Bob would lose 7 happiness units by sitting next to Carol.
Bob would lose 63 happiness units by sitting next to David.
Carol would lose 62 happiness units by sitting next to Alice.
Carol would gain 60 happiness units by sitting next to Bob.
Carol would gain 55 happiness units by sitting next to David.
David would gain 46 happiness units by sitting next to Alice.
David would lose 7 happiness units by sitting next to Bob.
David would gain 41 happiness units by sitting next to Carol.", 330)]
		public int Execute(string inputs)
		{
			LoadData(inputs, out var infos, out var circle);
			return Solve(infos, circle);
		}

		public static int Solve(Dictionary<(string, string), int> infos, ImmutableArray<string> circle)
		{
			var allCircles = Utilities.EachCircle(circle);
			var maxGain = allCircles.Max(circle => EvalCircle(circle, infos));
			return maxGain;
		}

		public static void LoadData(string inputs, out Dictionary<(string, string), int> infos, out ImmutableArray<string> circle)
		{
			infos = new Dictionary<(string, string), int>();
			foreach (var (First, Sign, Mag, Second) in inputs.SplitLines().Select(line => LineParser(line).Value))
				infos.Add((First, Second), Sign ? Mag : -Mag);
			circle = infos.Keys.Select(v => v.Item1).Distinct().ToImmutableArray();
		}
	}

	[Problem("2015-13-2")]
	public class Day13Part2
	{
		[TestCase(@"
Alice would gain 54 happiness units by sitting next to Bob.
Alice would lose 79 happiness units by sitting next to Carol.
Alice would lose 2 happiness units by sitting next to David.
Bob would gain 83 happiness units by sitting next to Alice.
Bob would lose 7 happiness units by sitting next to Carol.
Bob would lose 63 happiness units by sitting next to David.
Carol would lose 62 happiness units by sitting next to Alice.
Carol would gain 60 happiness units by sitting next to Bob.
Carol would gain 55 happiness units by sitting next to David.
David would gain 46 happiness units by sitting next to Alice.
David would lose 7 happiness units by sitting next to Bob.
David would gain 41 happiness units by sitting next to Carol.", 286)]
		public int Execute(string inputs)
		{
			Day13.LoadData(inputs, out var infos, out var circle);
			// Add self
			foreach (var other in circle)
			{
				infos.Add(("", other), 0);
				infos.Add((other, ""), 0);
			}
			circle = circle.Add("");
			return Day13.Solve(infos, circle);
		}
	}
}
