using System;
using ProblemsLibrary;

namespace AdventOfCode._2015;

[Problem("2015-20-01")]
public class Day20_Part1
{
    [TestCase("50", 4)]
    [TestCase("130", 8)]
    public int Execute(string input)
    {
        var minDivisorSum = int.Parse(input) / 10;
        var houses = new int[minDivisorSum + 1];
        for (var i = 1; i < houses.Length; ++i)
        {
            for (var j = i; j < houses.Length; j += i) houses[j] += i;
            if (houses[i] >= minDivisorSum)
                return i;
        }

        throw new Exception();
    }
}

[Problem("2015-20-02")]
public class Day20_Part2
{
    public int Execute(string input)
    {
        var minDivisorSum = int.Parse(input) / 11;
        var houses = new int[minDivisorSum + 1];
        for (var i = 1; i < houses.Length; ++i)
        {
            for (var j = i; j < Math.Min(50 * i, houses.Length); j += i) houses[j] += i;
            if (houses[i] >= minDivisorSum)
                return i;
        }

        throw new Exception();
    }
}