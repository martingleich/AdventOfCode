using System.Collections.Generic;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;
// ReSharper disable StringLiteralTypo

namespace AdventOfCode._2016;

[Problem("2016-06-01", MethodName = nameof(ExecutePart1))]
[Problem("2016-06-02", MethodName = nameof(ExecutePart2))]
public class Day6
{
    private static string Execute(string input, bool modified)
    {
        var lines = input.SplitLines().ToArray();
        var counters = Enumerable.Range(0, lines[0].Length).Select(_ => new Dictionary<char, int>()).ToArray();
        foreach (var line in lines)
        {
            for (var i = 0; i < line.Length; ++i)
            {
                counters[i].TryGetValue(line[i], out var count);
                ++count;
                counters[i][line[i]] = count;
            }
        }

        var result = new char[counters.Length];
        for (var i = 0; i < counters.Length; ++i)
            result[i] = (modified ? counters[i].MinBy(x => x.Value) : counters[i].MaxBy(x => x.Value)).Key;
        return new string(result);
    }

    [TestCase(@"
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
enarar", "easter")]
    public static string ExecutePart1(string input) => Execute(input, false);
    [TestCase(@"
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
enarar", "advent")]
    public static string ExecutePart2(string input) => Execute(input, true);
}