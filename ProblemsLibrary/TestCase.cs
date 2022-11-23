using System;

namespace ProblemsLibrary
{
	public sealed class TestCase
	{
		public readonly string Name;
		public readonly Func<TestCaseResult> Executor;

		public TestCase(string name, Func<TestCaseResult> executor)
		{
			Name = name;
			Executor = executor;
		}

		public override string ToString() => Name;
	}
}
