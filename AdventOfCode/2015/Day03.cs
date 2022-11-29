using AdventOfCode.Utils;
using ProblemsLibrary;
using System.Collections.Generic;
using System.Linq;

namespace AventOfCode._2015
{
    [Problem("2015-03-1")]
	public class Day03
	{
		public static (int, int) Move((int, int) cur, char c) => c switch
		{
			'^' => (cur.Item1, cur.Item2 + 1),
			'<' => (cur.Item1 - 1, cur.Item2),
			'>' => (cur.Item1 + 1, cur.Item2),
			'v' => (cur.Item1, cur.Item2 - 1),
			_ => cur,
		};
		[TestCase(">", 2)]
		[TestCase("^>v<", 4)]
		[TestCase("^v^v^v^v^v", 2)]
		public int Execute(string input) => input.RunningAggregate((0, 0), Move).Distinct().Count();
	}

	[Problem("2015-03-2")]
	public class Day03Part2
	{
		private IEnumerable<(int, int)> GetWayPoints(string input)
		{
			var santaPos = (0, 0);
			var roboSantaPos = (0, 0);
			yield return santaPos;
			bool robo = false;
			foreach (var c in input)
			{
				if (robo)
				{
					santaPos = Day03.Move(santaPos, c);
					yield return santaPos;
				}
				else
				{
					roboSantaPos = Day03.Move(roboSantaPos, c);
					yield return roboSantaPos;
				}
				robo = !robo;
			}
		}
		[TestCase("^v", 3)]
		[TestCase("^>v<", 3)]
		[TestCase("^v^v^v^v^v", 11)]
		public int Execute(string input) => GetWayPoints(input).Distinct().Count();
	}
}
