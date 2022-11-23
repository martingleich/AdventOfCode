using System;
using System.Linq;

namespace ProblemsLibrary
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class TestCaseAttribute : Attribute
	{
		public object[] Inputs { get; }
		public object Output { get; }
		public string? Condition { get; set; }
		public TestCaseAttribute(params object[] args)
		{
			Output = args[^1];
			Inputs = args.Take(args.Length - 1).ToArray();
		}
	}
}
