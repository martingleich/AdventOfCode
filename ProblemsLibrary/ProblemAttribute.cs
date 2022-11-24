using System;

namespace ProblemsLibrary
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class ProblemAttribute : Attribute
	{
		public ProblemAttribute(string id)
		{
			Id = id;
		}

		public string Id { get; }
	}
}
