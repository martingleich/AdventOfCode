using System.Text;
using ProblemsLibrary;

namespace AventOfCode._2015;

[Problem("2015-10-1")]
internal class Day10
{
    public string Iterate(string input)
    {
        var sb = new StringBuilder();
        var lastChar = '\0';
        var lastCount = 0;
        foreach (var c in input)
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
        for (var i = 0; i < count; ++i)
            input = Iterate(input);
        return input.Length;
    }

    public int Execute(string input)
    {
        return Execute(40, input);
    }
}