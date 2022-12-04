using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AdventOfCode._2022;

[Problem("2022-03-01", MethodName = nameof(ExecutePart1))]
[Problem("2022-03-02", MethodName = nameof(ExecutePart2))]
public class Day03
{
    private static int GetPriority(char c)
    {
        if (c >= 'a' && c <= 'z')
            return c - 'a' + 1;
        else if (c >= 'A' && c <= 'Z')
            return c - 'A' + 27;
        else
            throw new ArgumentException($"Invalid {nameof(c)}({c}).");
    }
    private const string TEST_DATA = @"
vJrwpWtwJgWrhcsFMMfFFhFp
jqHRNqRjqzjGDLGLrsFMfFZSrLrFZsSL
PmmdzqPrVvPwwTWBwg
wMqvLMZHhHMvwLHjbvcjnnSBnvTQFn
ttgJtRGJQctTZtZT
CrZsJsPPZsGzwwsLwLmpwMDw";
    [TestCase(TEST_DATA, 157)]
    public static int ExecutePart1(string input) => input.SplitLines().Select(line =>
        {
            var leftPart = line[..(line.Length / 2)];
            var rightPart = line[(line.Length / 2)..];
            return leftPart.Intersect(rightPart).Select(GetPriority).Sum();
        }).Sum();
    [TestCase(TEST_DATA, 70)]
    public static int ExecutePart2(string input) => input.SplitLines().Chunk(3).Select(lines =>
        {
            var badge = lines.IntersectAll().Single();
            return GetPriority(badge);
        }).Sum();
}
