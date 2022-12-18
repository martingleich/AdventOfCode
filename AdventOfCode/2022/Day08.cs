using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AdventOfCode._2022;

[Problem("2022-08-01", MethodName = nameof(ExecutePart1))]
[Problem("2022-08-02", MethodName = nameof(ExecutePart2))]
public class Day08
{
    private const string TestData = @"30373
25512
65332
33549
35390";

    [TestCase(TestData, 21)]
    public static int ExecutePart1(string input)
    {
        var grid = Parser.SingleDigit.Grid(Parser.NewLine).Parse(input.Trim());
        return Matrix.CombineMany(MathBool.Or, grid.FromEachDirection(GetVisibility)).GetValues().Count(x => x);
    }

    [TestCase(TestData, 8)]
    public static int ExecutePart2(string input)
    {
        var grid = Parser.SingleDigit.Grid(Parser.NewLine).Parse(input.Trim());
        return Matrix.CombineMany(MathInt.Mul, grid.FromEachDirection(GetVisibleCount)).GetValues().Max();
    }

    private static Vector<int> GetVisibleCount<T>(Vector<T> values) where T : IComparable<T>
    {
        var result = new List<int>();
        for (var i = 0; i < values.Length; ++i)
        {
            var count = 0;
            var height = values[i];
            for (var j = i + 1; j < values.Length; ++j)
            {
                ++count;
                if (values[j].CompareTo(height) >= 0)
                    break;
            }

            result.Add(count);
        }

        return Vector.FromEnumerable(result);
    }

    private static Vector<bool> GetVisibility<T>(Vector<T> values) where T : IComparable<T>
    {
        var result = new List<bool>();
        if (values.Length > 0)
        {
            result.Add(true);
            var max = values[0];
            for (var i = 1; i < values.Length; ++i)
            {
                var isVisible = values[i].CompareTo(max) > 0;
                if (isVisible)
                    max = values[i];
                result.Add(isVisible);
            }
        }

        return Vector.FromEnumerable(result);
    }
}