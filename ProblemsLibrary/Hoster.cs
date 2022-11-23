using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ProblemsLibrary
{
	public sealed class Hoster
	{
		private readonly ImmutableArray<Problem> Problems;
		private readonly Func<string, string> DefaultInputFileProvider;

		public Hoster(
			ImmutableArray<Problem> problems,
			Func<string, string> defaultInputFileProvider)
		{
			Problems = problems;
			DefaultInputFileProvider = defaultInputFileProvider;
		}

		private Problem? TryGetProblem(string id) => Problems.FirstOrDefault(p => p.Id == id);

		public bool RunTests(string id) => RunTests(Problems.Where(p => p.Id == id));

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

		public void SolveProblem(string id, string input)
		{
			var problem = TryGetProblem(id) ?? throw new ArgumentException();
			var inputData = TryReadInput(problem, input) ?? throw new ArgumentException();
			SolveProblem(problem, inputData);
		}

		public void RunUserInteractive()
		{
			while (true)
			{
				var problem = ConsoleHelper.Prompt("Enter the index of the task: ", TryGetProblem);
				var inputData = ConsoleHelper.Prompt("Enter the input of the task: ", input => TryReadInput(problem, input));
				SolveProblem(problem, inputData);
			}
		}

		private string? TryReadInput(Problem problem, string input)
		{
			if (input.StartsWith("//"))
			{
				var path = input[2..];
				if (path == "")
					path = DefaultInputFileProvider(problem.Id);

				return Utils.TryReadAllText(path);
			}
			return input;
		}

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
	}
}
