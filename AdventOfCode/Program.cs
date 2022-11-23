using System;
using System.Linq;

namespace AdventOfCode
{
	public static class Program
	{
		static void Main(string[] args)
		{
			var hoster = new ProblemsLibrary.Hoster(
				ProblemsLibrary.Reflection.FindAllProblems(typeof(Program).Assembly),
				id => $"Inputs\\{(id.Count(c => c == '-') == 2 && id.EndsWith("-2") ? id.Substring(0, id.Length - 2) : id)}.txt");
			hoster.RunTests();
			hoster.RunUserInteractive();
		}
	}
}
