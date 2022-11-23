using System;
using System.Collections.Immutable;

namespace ProblemsLibrary
{
	public sealed class Problem
	{
		public readonly string Id;
		public readonly ImmutableArray<TestCase> TestCases;
		public readonly Func<string, object> Execute;

		public Problem(string id, ImmutableArray<TestCase> testCases, Func<string, object> execute)
		{
			Id = id;
			TestCases = testCases;
			Execute = execute;
		}

		public override string ToString() => Id;
	}
}
