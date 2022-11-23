using ProblemsLibrary;
using System;
using System.Collections.Immutable;
using System.Linq;
using TypesafeParser;

namespace AventOfCode._2015
{
	[Problem("2015-14")]
	public class Day14
	{
		public class Reindeer
		{
			public readonly int Speed;
			public readonly int FlyTime;
			public readonly int RestTime;

			public Reindeer(int speed, int flyTime, int restTime)
			{
				Speed = speed;
				FlyTime = flyTime;
				RestTime = restTime;
			}

			public int DistancePerPeriod => Speed * FlyTime;
			public int PeriodTime => FlyTime + RestTime;

			public int GetDistanceAtTime(int time)
			{
				var periods = time / PeriodTime;
				var timeInPeriod = time - periods * PeriodTime;
				var flyTimeInPeriod = Math.Min(FlyTime, timeInPeriod);
				return periods * DistancePerPeriod + flyTimeInPeriod * Speed;
			}

			private static Func<string, ErrorOr<(string, int, int, int)>> Parser = Pattern.Empty.
				AlphaNumeric().
				Fixed(" can fly ").
				AlphaNumeric(int.Parse).
				Fixed(" km/s for ").
				AlphaNumeric(int.Parse).
				Fixed(" seconds, but then must rest for ").
				AlphaNumeric(int.Parse).
				Fixed(" seconds.").
				EndOfInput().
				Compile();

			public static Reindeer FromText(string line)
			{
				var value = Parser(line).Value;
				return new Reindeer(value.Item2, value.Item3, value.Item4);
			}
		}

		[TestCase(@"Comet can fly 14 km/s for 10 seconds, but then must rest for 127 seconds.
Dancer can fly 16 km/s for 11 seconds, but then must rest for 162 seconds.", 1000, 1120)]
		public int InternalExecute(string input, int time)
		{
			var reindeers = input.SplitLines().Select(Reindeer.FromText);
			var flyTimes = reindeers.Select(r => r.GetDistanceAtTime(time));
			return flyTimes.Max();
		}

		public int Execute(string input) => InternalExecute(input, 2503);
	}

	[Problem("2015-14-2")]
	public class Day14Part2
	{
		public class State
		{
			public readonly Day14.Reindeer Reindeer;
			private int Timer;
			private bool IsResting;
			public int Points;

			public State(Day14.Reindeer reindeer)
			{
				Reindeer = reindeer;
				// Start flying
				Timer = Reindeer.FlyTime;
			}

			public int Distance { get; private set; }

			public void Step()
			{
				if (!IsResting)
					Distance += Reindeer.Speed;
				Timer -= 1;
				if (Timer == 0)
				{
					IsResting = !IsResting;
					if (IsResting)
						Timer = Reindeer.RestTime;
					else
						Timer = Reindeer.FlyTime;
				}
			}
		}
		[TestCase(@"Comet can fly 14 km/s for 10 seconds, but then must rest for 127 seconds.
Dancer can fly 16 km/s for 11 seconds, but then must rest for 162 seconds.", 1000, 689)]
		public int InternalExecute(string input, int time)
		{
			var states = input.SplitLines().Select(Day14.Reindeer.FromText).Select(r => new State(r)).ToImmutableArray();
			for (int i = 0; i < time; ++i)
			{
				foreach (var state in states)
					state.Step();
				var furthestDistance = states.Max(s => s.Distance);
				foreach (var s in states.Where(s => s.Distance == furthestDistance))
					s.Points += 1;
			}
			return states.Max(s => s.Points);
		}
		public int Execute(string input) => InternalExecute(input, 2503);
	}
}
