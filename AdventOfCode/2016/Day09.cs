using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AdventOfCode.Utils;
using ProblemsLibrary;
// ReSharper disable StringLiteralTypo

namespace AdventOfCode._2016;

[Problem("2016-09-01", MethodName = nameof(ExecutePart1))]
[Problem("2016-09-02", MethodName = nameof(ExecutePart2))]
public class Day09
{
    private static readonly Parser<(int, int)> patternParser = Parser.MakeRegexParser(new Regex(@"(\d+)x(\d+)"), m => (int.Parse(m.Groups[1].ValueSpan), int.Parse(m.Groups[2].ValueSpan)));

    [TestCase("ADVENT", false, 6ul)]
    [TestCase("A(1x5)BC", false, 7ul)]
    [TestCase("(3x3)XYZ", false, 9ul)]
    [TestCase("A(2x2)BCD(2x2)EFG", false, 11ul)]
    [TestCase("(6x1)(1x3)A", false, 6ul)]
    [TestCase("X(8x2)(3x3)ABCY", false, 18ul)]
    [TestCase("X(8x2) (3x3)ABCY", false, 18ul)]
    [TestCase("ADVENT", true, 6ul)]
    [TestCase("(3x3)XYZ", true, 9ul)]
    [TestCase("X(8x2)(3x3)ABCY", true, 20ul)]
    [TestCase("(27x12)(20x12)(13x14)(7x10)(1x12)A", true, 241920ul)]
    [TestCase("(25x3)(3x3)ABC(2x3)XY(5x2)PQRSTX(18x9)(3x2)TWO(5x7)SEVEN", true, 445ul)]
    public static ulong EvalLen(string input, bool recursive)
    {
        // Remove whitespace
        input = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());

        var evalStack = new Stack<(int, Span)>();
        evalStack.Push((1, Span.FromString(input)));
        ulong length = 0;
        while (evalStack.TryPop(out var eval))
        {
            var (mul, value) = eval;
            var subLength = 0ul;
            for (int i = 0; i < value.Length; ++i)
            {
                var c = value[i];
                if (c == '(')
                {
                    int start = i + 1;
                    while (i < value.Length && value[i] != ')')
                        ++i;
                    var pattern = patternParser.Parse(value.Substring(start, i));
                    var repeated = value.Substring(i + 1, i + 1 + pattern.Item1);
                    if (recursive)
                        evalStack.Push((mul * pattern.Item2, repeated));
                    else
                        subLength += (ulong)(pattern.Item2 * repeated.Length);
                    i += pattern.Item1;
                }
                else
                {
                    ++subLength;
                }
            }
            length += (ulong)mul * subLength;
        }
        return length;
    }
    public static ulong ExecutePart1(string input) => EvalLen(input, false);
    public static ulong ExecutePart2(string input) => EvalLen(input, true);
}
