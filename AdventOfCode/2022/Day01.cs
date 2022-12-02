using AdventOfCode.Utils;
using ProblemsLibrary;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS8509
#pragma warning disable CS8524

namespace AdventOfCode._2022;

[Problem("2022-01-01", MethodName = nameof(ExecutePart1))]
[Problem("2022-01-02", MethodName = nameof(ExecutePart2))]
public class Day01
{
    private const string TEST_DATA = @"1000
2000
3000

4000

5000
6000

7000
8000
9000

10000
";

    [TestCase(TEST_DATA, 24000)]
    public static int ExecutePart1(string input) => AllCalories(input).Max();
    [TestCase(TEST_DATA, 45000)]
    public static int ExecutePart2(string input) => AllCalories(input).OrderByDescending(x => x).Take(3).Sum();

    private static IEnumerable<int> AllCalories(string input) => input
                .SplitLines(false)
                .Split(string.IsNullOrWhiteSpace)
                .Select(elf => elf.Sum(int.Parse));
}