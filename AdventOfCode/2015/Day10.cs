using ProblemsLibrary;
using System.Text;

namespace AventOfCode._2015
{
	[Problem("2015-10-1")]
	class Day10
	{
		public string Iterate(string input)
		{
			var sb = new StringBuilder();
			char lastChar = '\0';
			int lastCount = 0;
			foreach (var c in input)
			{
				if (c == lastChar)
				{
					++lastCount;
				}
				else
				{
					if (lastCount > 0)
					{
						sb.Append(lastCount);
						sb.Append(lastChar);
					}
					lastChar = c;
					lastCount = 1;
				}
			}
			if (lastCount > 0)
			{
				sb.Append(lastCount);
				sb.Append(lastChar);
			}
			return sb.ToString();
		}
		[TestCase(4, "1", 6)]
		public int Execute(int count, string input)
		{
			for (int i = 0; i < count; ++i)
				input = Iterate(input);
			return input.Length;
		}
		public int Execute(string input) => Execute(40, input);
	}
}
