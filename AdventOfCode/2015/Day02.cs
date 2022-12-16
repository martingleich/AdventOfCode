using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AventOfCode._2015;

[Problem("2015-02-1")]
public sealed class Day02
{
    [TestCase("2x3x4", 58)]
    [TestCase("1x1x10", 43)]
    private int SingleBox(string input)
    {
        var box = Box.FromText(input);
        var side1 = box.Dimension1 * box.Dimension2;
        var side2 = box.Dimension1 * box.Dimension3;
        var side3 = box.Dimension2 * box.Dimension3;
        return 2 * (side1 + side2 + side3) + Utilities.Min(side1, side2, side3);
    }

    [TestCase("1x1x10\n2x3x4", 101)]
    public int Execute(string input)
    {
        return input.SplitLines().Sum(SingleBox);
    }

    public sealed class Box
    {
        public readonly int Dimension1;
        public readonly int Dimension2;
        public readonly int Dimension3;

        public Box(int dimension1, int dimension2, int dimension3)
        {
            Dimension1 = dimension1;
            Dimension2 = dimension2;
            Dimension3 = dimension3;
        }

        public int Volume => Dimension1 * Dimension2 * Dimension3;

        public static Box FromText(string str)
        {
            var dimensions = str.Split("x").Select(int.Parse).ToArray();
            return new Box(dimensions[0], dimensions[1], dimensions[2]);
        }
    }
}

[Problem("2015-02-2")]
public sealed class Day02Part2
{
    [TestCase("2x3x4", 34)]
    [TestCase("1x1x10", 14)]
    private int SingleBox(string input)
    {
        var box = Day02.Box.FromText(input);
        var halfPerim1 = box.Dimension1 + box.Dimension2;
        var halfPerim2 = box.Dimension1 + box.Dimension3;
        var halfPerim3 = box.Dimension2 + box.Dimension3;
        var smallestHalfPerim = Utilities.Min(halfPerim1, halfPerim2, halfPerim3);
        return 2 * smallestHalfPerim + box.Volume;
    }

    public int Execute(string input)
    {
        return input.SplitLines().Sum(SingleBox);
    }
}