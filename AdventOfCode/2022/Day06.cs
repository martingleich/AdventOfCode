using System;
using System.Collections.Generic;
using ProblemsLibrary;

namespace AdventOfCode._2022;

[Problem("2022-06-01", MethodName = nameof(ExecutePart1))]
[Problem("2022-06-02", MethodName = nameof(ExecutePart2))]
public class Day06
{
    private static int ExecuteCommon(string input, int len)
    {
        var counts = new Dictionary<char, int>();
        var diffCount = 0;
        for (var i = 0; i < input.Length; ++i)
        {
            if (i - len >= 0)
            {
                counts.TryGetValue(input[i - len], out var oldCount);
                counts[input[i - len]] = oldCount - 1;
                if (oldCount == 1)
                    --diffCount;
            }
            counts.TryGetValue(input[i], out var count);
            counts[input[i]] = count + 1;
            if (count == 0)
                ++diffCount;
            if (diffCount == len)
                return 1 + i;
        }
        throw new InvalidOperationException();
    }
    [TestCase("mjqjpqmgbljsphdztnvjfqwrcgsmlb", 7)]
    [TestCase("bvwbjplbgvbhsrlpgdmjqwftvncz", 5)]
    [TestCase("nppdvjthqldpwncqszvftbrmjlhg", 6)]
    [TestCase("nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg", 10)]
    [TestCase("zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw", 11)]
    public static int ExecutePart1(string input) => ExecuteCommon(input, 4);
    [TestCase("mjqjpqmgbljsphdztnvjfqwrcgsmlb", 19)]
    [TestCase("bvwbjplbgvbhsrlpgdmjqwftvncz", 23)]
    [TestCase("nppdvjthqldpwncqszvftbrmjlhg", 23)]
    [TestCase("nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg", 29)]
    [TestCase("zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw", 26)]
    public static int ExecutePart2(string input) => ExecuteCommon(input, 14);
}
