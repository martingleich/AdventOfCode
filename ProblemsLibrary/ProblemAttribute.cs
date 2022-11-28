using System;

namespace ProblemsLibrary
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ProblemAttribute : Attribute
	{
		public ProblemAttribute(string id)
		{
			Id = id;
		}

		public string Id { get; }
		public string? MethodName { get; set; }
	}
}
