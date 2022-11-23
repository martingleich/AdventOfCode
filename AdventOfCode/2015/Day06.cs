using ProblemsLibrary;
using System;
using System.Linq;

namespace AventOfCode._2015
{
	[Problem("2015-06-1")]
	public class Day06
	{
		public readonly struct Point
		{
			public readonly int X;
			public readonly int Y;

			public Point(int x, int y)
			{
				X = x;
				Y = y;
			}

			public static Point FromString(string v)
			{
				var values = v.Split(",").Select(int.Parse).ToArray();
				return new Point(values[0], values[1]);
			}
		}
		public readonly struct Rect
		{
			public readonly Point LowerLeft;
			public readonly Point UpperRight;

			public Rect(Point lowerLeft, Point upperRight)
			{
				LowerLeft = lowerLeft;
				UpperRight = upperRight;
			}
			public bool Contains(Point p) =>
				p.X >= LowerLeft.X && p.X <= UpperRight.X &&
				p.Y >= LowerLeft.Y && p.Y <= UpperRight.Y;
		}

		public sealed class Action
		{
			public readonly Rect Rect;
			public readonly bool Toogle;
			public readonly bool Value;

			public Action(Rect rect, bool toogle, bool value)
			{
				Rect = rect;
				Toogle = toogle;
				Value = value;
			}

			public void Apply(bool[] grid, int pitch)
			{
				for (int y = Rect.LowerLeft.Y; y <= Rect.UpperRight.Y; ++y)
				{
					for (int x = Rect.LowerLeft.X; x <= Rect.UpperRight.X; ++x)
					{
						grid[y * pitch + x] = Toogle ? !grid[y * pitch + x] : Value;
					}
				}
			}

			public static Action FromString(string input)
			{
				bool toogle, value;
				var words = input.Split(" ");
				int cur = 0;
				if (words[cur++] == "turn")
				{
					toogle = false;
					value = words[cur++] == "on";
				}
				else // toogle
				{
					toogle = true;
					value = false;
				}
				var lower = Point.FromString(words[cur++]);
				++cur; // Skip "through"
				var upper = Point.FromString(words[cur]);
				return new Action(new Rect(lower, upper), toogle, value);
			}
		}

		[TestCase("turn on 0,0 through 0,0", 1)]
		[TestCase("turn on 0,0 through 1,1", 4)]
		[TestCase("turn on 0,0 through 1,2", 6)]
		[TestCase("turn on 1,1 through 1,2", 2)]
		[TestCase("turn on 1,0 through 1,2", 3)] // Various turn ons to test positions
		[TestCase("turn on 0,0 through 0,0\nturn on 0,0 through 0,0", 1)]// Turn on already on.
		[TestCase("turn off 0,0 through 0,0", 0)] // Turn off already off
		[TestCase("turn on 0,0 through 0,0\nturn off 0,0 through 0,0", 0)] // Turn off already on
		[TestCase("turn on 0,0 through 0,0\ntoggle 0,0 through 0,0", 0)] // Toogle off
		[TestCase("toggle 0,0 through 0,0", 1)] // Toogle on
		public int Execute(string input)
		{
			var actions = input.SplitLines().Select(Action.FromString);
			var grid = new bool[1000 * 1000];
			foreach (var action in actions)
				action.Apply(grid, 1000);
			return grid.Count(x => x);
		}
	}

	[Problem("2015-06-2")]
	public class Day06Part2
	{
		public sealed class Action
		{
			public readonly Day06.Rect Rect;
			public readonly int Change;

			public Action(Day06.Rect rect, int change)
			{
				Rect = rect;
				Change = change;
			}

			public void Apply(int[] grid, int pitch)
			{
				for (int y = Rect.LowerLeft.Y; y <= Rect.UpperRight.Y; ++y)
				{
					for (int x = Rect.LowerLeft.X; x <= Rect.UpperRight.X; ++x)
					{
						grid[y * pitch + x] = Math.Max(grid[y * pitch + x] + Change, 0);
					}
				}
			}

			public static Action FromString(string input)
			{
				int value;
				var words = input.Split(" ");
				int cur = 0;
				if (words[cur++] == "turn")
				{
					value = words[cur++] == "on" ? 1 : -1;
				}
				else // toogle
				{
					value = 2;
				}
				var lower = Day06.Point.FromString(words[cur++]);
				++cur; // Skip "through"
				var upper = Day06.Point.FromString(words[cur]);
				return new Action(new Day06.Rect(lower, upper), value);
			}
		}

		[TestCase("turn on 0,0 through 0,0", 1)]
		[TestCase("turn on 0,0 through 1,1", 4)]
		[TestCase("turn on 0,0 through 1,2", 6)]
		[TestCase("turn on 1,1 through 1,2", 2)]
		[TestCase("turn on 1,0 through 1,2", 3)] // Various turn ons to test positions
		[TestCase("turn on 0,0 through 0,0\nturn on 0,0 through 0,0", 2)]// Turn on already on.
		[TestCase("turn off 0,0 through 0,0", 0)] // Turn off already off
		[TestCase("turn on 0,0 through 0,0\nturn off 0,0 through 0,0", 0)] // Turn off already on
		[TestCase("turn on 0,0 through 0,0\ntoggle 0,0 through 0,0", 3)] // Toogle off
		[TestCase("toggle 0,0 through 0,0", 2)] // Toogle on
		public int Execute(string input)
		{
			var actions = input.SplitLines().Select(Action.FromString);
			var grid = new int[1000 * 1000];
			foreach (var action in actions)
				action.Apply(grid, 1000);
			return grid.Sum();
		}
	}
}
