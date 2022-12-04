using System.Collections.Generic;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;
// ReSharper disable StringLiteralTypo

namespace AdventOfCode._2016;

[Problem("2016-07-01", MethodName = nameof(ExecutePart1))]
[Problem("2016-07-02", MethodName = nameof(ExecutePart2))]
public class Day07
{
    [TestCase("abba[mnop]qrst", true)]
    [TestCase("abcd[bddb]xyyx", false)]
    [TestCase("aaaa[qwer]tyui", false)]
    [TestCase("ioxxoj[asdfgh]zxcvbn", true)]
    [TestCase("ioxxoj[asd]abc[fgh]zxcvbn", true)]
    public static bool SupportsTLS(string input)
    {
        var (inside, outside) = SplitInsideOutsideBrackets(input);
        return outside.Any(HasABBA) && !inside.Any(HasABBA);
    }
    [TestCase("aba[bab]xyz", true)]
    [TestCase("xyx[xyx]xyx", false)]
    [TestCase("aaa[kek]eke", true)]
    [TestCase("zazbz[bzb]cdb", true)]
    public static bool SupportsSSL(string input)
    {
        var (inside, outside) = SplitInsideOutsideBrackets(input);
        var allABAsOutside = outside.SelectMany(GetABAs);
        var allABAsInside = inside.SelectMany(GetABAs);
        var matchingForOutside = allABAsOutside.Select(x => $"{x[1]}{x[0]}{x[1]}").ToArray();
        return allABAsInside.Intersect(matchingForOutside).Any();
    }
    public static bool HasABBA(string input)
    {
        for (int i = 0; i < input.Length - 3; ++i)
            if (input[i] != input[i + 1] && input[i + 1] == input[i + 2] && input[i] == input[i + 3])
                return true;
        return false;
    }
    public static IEnumerable<string> GetABAs(string input)
    {
        for (int i = 0; i < input.Length - 2; ++i)
            if (input[i] != input[i + 1] && input[i] == input[i + 2])
                yield return input[i..(i + 3)];
    }

    private static (string[], string[]) SplitInsideOutsideBrackets(string input)
    {
        var s = "";
        var depth = 0;
        var outside = new List<string>();
        var inside = new List<string>();
        foreach (char c in input)
        {
            if (c == '[')
            {
                if (s != "")
                {
                    outside.Add(s);
                    s = "";
                }
                ++depth;
            }
            else if (c == ']')
            {
                if (s != "")
                {
                    inside.Add(s);
                    s = "";
                }
                --depth;
            }
            else
                s += c;
        }
        if (s != "")
            outside.Add(s);
        return (inside.ToArray(), outside.ToArray());
    }
    
    public static int ExecutePart1(string input) => input.SplitLines().Count(SupportsTLS);
    public static int ExecutePart2(string input) => input.SplitLines().Count(SupportsSSL);
}
