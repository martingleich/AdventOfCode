using System;

namespace ProblemsLibrary
{
	public class ProblemAttribute : Attribute
	{
		public ProblemAttribute(string id)
		{
			Id = id;
		}

		public string Id { get; }
	}
}
