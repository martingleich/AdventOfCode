using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace ProblemsLibrary
{
	public sealed class Hoster
	{
		private readonly ImmutableArray<Problem> _problems;

		public Hoster(ImmutableArray<Problem> problems) {
			_problems = problems;
		}

		public Problem? GetProblem(string id) =>
			_problems.FirstOrDefault(p => p.Id == id);
		public IEnumerable<Problem> GetProblems(string s)
		{
			var parts = s.Split('-').ToArray();
			return from problem in _problems
				let parts2 = problem.Id.Split('-').ToArray()
				where parts2.Length >= parts.Length && !parts.Where((t, i) => t != "*" && t != parts2[i]).Any()
				select problem;
		}

		public void Run(Problem problems, string input)
		{
			if (RunTests(new []{problems}))
				SolveProblems(new []{problems}, input);
		}

		private static bool RunTests(IEnumerable<Problem> problems)
		{
			var start = DateTime.Now;
			int count = 0;
			int countFailed = 0;
			foreach (var prob in problems)
			{
				bool firstTest = true;
				foreach (var test in prob.TestCases)
				{
					if (firstTest)
					{
						Console.WriteLine("=========================================================");
						Console.WriteLine($"Running tests for {prob.Id}:");
						firstTest = false;
					}
					++count;
					Console.Write($"\t{test.Name} => ");
					var result = test.Executor();
					if (result.Failed(out var msg))
					{
						Console.WriteLine($"FAILED\n\t\t{msg}");
						++countFailed;
					}
					else
						Console.WriteLine($"SUCCEEDED");
				}
			}
			var duration = DateTime.Now - start;
			Console.WriteLine("=========================================================");
			Console.WriteLine($"Succeeded: {count - countFailed} Failed: {countFailed}");
			Console.WriteLine($"Finished in {duration}");
			return countFailed == 0;
		}

		private void SolveProblems(IEnumerable<Problem> problems, string input)
		{
			foreach (var problem in problems)
			{
				var inputData = TryReadInput(problem, input) ?? throw new ArgumentException();
				SolveProblem(problem, inputData);
			}
		}

		private static string? TryReadInput(Problem problem, string input) => input.StartsWith("//") ? Utils.TryReadAllText(input[2..]) : input;

		private static void SolveProblem(Problem problem, string inputData)
		{
            var start = DateTime.Now;
            object result;
            try
            {
                result = problem.Execute(inputData);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}");
                return;
            }
            var duration = DateTime.Now - start;
            Console.WriteLine($"Problem:{problem.Id} Input:{inputData}\nResult = {result}. Calculated in {duration}.");
		}

		public void Profile(ImmutableArray<KeyValuePair<Problem, string>> requests, int repeatCount)
		{
			const int innerRepeatCount = 5;
			var watches = Enumerable.Range(0, requests.Length).Select(_ => new Stopwatch()).ToArray();
			for(var i = 0; i < repeatCount; ++i)
			{
				var id = 0;
				foreach (var (problem, input) in requests)
				{
					watches[id].Start();
					for(int j = 0; j < innerRepeatCount; ++j)
						problem.Execute(input);
					watches[id].Stop();
					++id;
				}
			}

			var times = watches.Select(w => w.Elapsed / (repeatCount * innerRepeatCount)).ToArray();
			var totalTime = times.Aggregate((a, b) => a + b);
			var lines = Enumerable.Range(0, times.Length).Select(i => new[]
			{
				requests[i].Key.Id,
				times[i].TotalMilliseconds.ToString(),
				$"{((times[i] / totalTime) * 100):F2} %",
			}).ToList();
			lines.Insert(0, new []{"Id", "Time(ms)", "Share"});
			foreach(var line in lines)
				for (int i = 0; i < line.Length; ++i)
					line[i] = $" {line[i]} ";
			var columnSizes = Enumerable.Range(0, 3).Select(i => lines.Max(l => l[i].Length)).ToArray();
			var sep = new string('-', columnSizes.Sum() + columnSizes.Length + 1);
			Console.WriteLine();
			Console.WriteLine(sep);
			foreach (var line in lines)
			{
				Console.Write("|");
				for (int i = 0; i < line.Length; ++i)
				{
					Console.Write(line[i].PadLeft(columnSizes[i]));
					Console.Write("|");
				}
				Console.WriteLine();
				Console.WriteLine(sep);
			}
		}

		public void Test(IEnumerable<Problem> problems)
		{
			RunTests(problems);
		}
	}
}
