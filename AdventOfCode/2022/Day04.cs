using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AdventOfCode._2022;

[Problem("2022-04-01", MethodName = nameof(ExecutePart1))]
[Problem("2022-04-02", MethodName = nameof(ExecutePart2))]
public class Day04
{
    private const string TEST_DATA = @"2-4,6-8
2-3,4-5
5-7,7-9
2-8,3-7
6-6,4-6
2-6,4-8";

    private readonly record struct Range(int From, int To)
    {
        public bool IsContainedIn(Range r) => From >= r.From && To <= r.To;
        public bool OverlapsWith(Range r)
        {
            var commonFrom = Math.Max(From, r.From);
            var commonTo = Math.Min(To, r.To);
            return commonFrom <= commonTo;

        }
    }
    private static IEnumerable<(Range, Range)> ParseInput(string input)
    {
        var parserRange = Parser.FormattedString($"{Parser.Int32}-{Parser.Int32}", m => new Range(m[0], m[1]));
        var parserElf = Parser.FormattedString<(Range, Range)>($"{parserRange},{parserRange}", m => (m[0], m[1]));
        return input.SplitLines().Select(parserElf.Parse);
    }
    [TestCase(TEST_DATA, 2)]
    public static int ExecutePart1(string input) => ParseInput(input).Count(ranges =>
        ranges.Item1.IsContainedIn(ranges.Item2) || ranges.Item2.IsContainedIn(ranges.Item1));
    [TestCase(TEST_DATA, 4)]
    public static int ExecutePart2(string input) => ParseInput(input).Count(ranges =>
             ranges.Item1.OverlapsWith(ranges.Item2));
}