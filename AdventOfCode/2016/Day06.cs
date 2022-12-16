using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;
// ReSharper disable StringLiteralTypo

namespace AdventOfCode._2016;

[Problem("2016-06-01", MethodName = nameof(ExecutePart1))]
[Problem("2016-06-02", MethodName = nameof(ExecutePart2))]
public class Day06
{
    private const string TestString = @"
eedadn
drvtee
eandsr
raavrd
atevrs
tsrnev
sdttsa
rasrtv
nssdts
ntnada
svetve
tesnvt
vntsnd
vrdear
dvrsen
enarar";

    private static string Execute(string input, bool modified)
    {
        var lines = input.SplitLines().ToArray();
        var matrix = Matrix.FromRows(lines);
        Func<Vector<char>, char>
            colSelect = modified ? h => h.GetValues().MostHistogram() : h => h.GetValues().LeastHistogram();
        return new string(matrix.GetColumns().Select(colSelect).ToArray());
    }

    [TestCase(TestString, "easter")]
    public static string ExecutePart1(string input) => Execute(input, false);
    [TestCase(TestString, "advent")]
    public static string ExecutePart2(string input) => Execute(input, true);
}
