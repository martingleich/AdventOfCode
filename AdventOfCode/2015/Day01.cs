using System;
using System.Linq;
using ProblemsLibrary;

namespace AventOfCode._2015;

[Problem("2015-01-1")]
public sealed class Day01
{
    public static int GetDirection(char c)
    {
        return c switch
        {
            '(' => 1,
            ')' => -1,
            _ => throw new ArgumentException($"Invalid character {c}")
        };
    }

    [TestCase("", 0)]
    [TestCase("(", 1)]
    [TestCase(")", -1)]
    [TestCase("(())", 0)]
    [TestCase("()()", 0)]
    [TestCase("(((", 3)]
    [TestCase("(()(()(", 3)]
    public int Execute(string input)
    {
        return input.Sum(GetDirection);
    }
}

[Problem("2015-01-2")]
public sealed class Day01Part2
{
    [TestCase(")", 1)]
    [TestCase("()())", 5)]
    [TestCase("(", -1)]
    public int Execute(string input)
    {
        var floor = 0;
        var index = 0;
        foreach (var c in input)
        {
            ++index;
            floor += Day01.GetDirection(c);
            if (floor < 0)
                return index;
        }

        return -1;
    }
}