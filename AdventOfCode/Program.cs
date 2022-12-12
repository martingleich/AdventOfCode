using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace AdventOfCode
{
	public static class Program
	{
		public static int Main(string[] args)
		{
			var mode = args[0];
			var hoster = new ProblemsLibrary.Hoster(ProblemsLibrary.Reflection.FindAllProblems(typeof(Program).Assembly));
			if (mode == "run")
			{
				var problem = hoster.GetProblem(args[1]);
				if (problem == null)
				{
					Console.Error.WriteLine($"Unknown problem: {args[1]}");
					return 1;
				}

				hoster.Run(problem, args[2]);
				return 0;
			} else if (mode == "profile")
			{
				string FilenameProvider(string id)
				{
					var parts = id.Split('-');
					return $"{parts[0]}-{parts[1]}.txt";
				}

				// Arg1: Problemlist
				// Arg2: Folder that contains the inputs for the problems named after the problem-id.txt
				// Arg3: RepeatCount -> 0 for infinite
				var repeatCount = int.Parse(args[3]);
				var problems = hoster.GetProblems(args[1]);
				var requests = (from problem in problems
						let inputPath = Path.Combine(args[2], FilenameProvider(problem.Id))
						let inputData = File.ReadAllText(inputPath)
						select KeyValuePair.Create(problem, inputData)).ToImmutableArray();
				hoster.Profile(requests, repeatCount);
				return 0;
			}
			
			Console.Error.WriteLine($"Unknown mode: {args[0]}");
			return 3;
		}
	}
}
