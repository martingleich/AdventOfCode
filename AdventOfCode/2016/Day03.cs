using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AdventOfCode._2016;

[Problem("2016-03-01", MethodName = nameof(ExecutePart1))]
[Problem("2016-03-02", MethodName = nameof(ExecutePart2))]
public class Day03
{
    [TestCase(@"
5 10 25
10 5 25
5 25 10
10 2 8
3 4 5
", 1)]
    public static int ExecutePart1(string input)
    {
        return ParseInput1(input).Count(IsValidTriangle);
    }

    public static int ExecutePart2(string input)
    {
        return ParseInput2(input).Count(IsValidTriangle);
    }

    private static bool IsValidTriangle(int[] line)
    {
        return line[0] < line[1] + line[2] &&
               line[1] < line[2] + line[0] &&
               line[2] < line[0] + line[1];
    }

    private static IEnumerable<int[]> ParseInput1(string input)
    {
        return input
            .SplitLines()
            .Select(x => x
                .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray());
    }

    private static IEnumerable<int[]> ParseInput2(string input)
    {
        return input
            .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToRectAndTranspose(3)
            .Chunk(3);
    }
}