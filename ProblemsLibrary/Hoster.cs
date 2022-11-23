using System;
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

		public void RunTests()
		{
			var start = DateTime.Now;
			int count = 0;
			int countFailed = 0;
			foreach (var prob in Problems)
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
		}

		public void RunUserInteractive()
		{
			while (true)
			{
				var problem = ConsoleHelper.Prompt("Enter the index of the task: ", input => Problems.FirstOrDefault(p => p.Id == input));
				var inputData = ConsoleHelper.Prompt("Enter the input of the task: ", input =>
				{
					if (input.StartsWith("//"))
					{
						var path = input[2..];
						if (path == "")
							path = DefaultInputFileProvider(problem.Id);

						return Utils.TryReadAllText(path);
					}
					return input;
				});
				var start = DateTime.Now;
				object result;
				try
				{
					result = problem.Execute(inputData);
				}
				catch (Exception e)
				{
					Console.WriteLine($"Exception: {e}");
					continue;
				}
				var duration = DateTime.Now - start;
				Console.WriteLine($"Result = {result}. Calculated in {duration}.");
			}
		}
	}
}
