using AdventOfCode.Utils;
using ProblemsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AdventOfCode._2016;

[Problem("2016-13-01", MethodName = nameof(ExecutePart1))]
[Problem("2016-13-02", MethodName = nameof(ExecutePart2))]
public class Day13
{
    private static Func<Vec2I, bool> IsSpace(int key) => v => v.X >= 0 && v.Y >= 0 && BitOperations.PopCount((uint)(v.X * v.X + 3 * v.X + 2 * v.X * v.Y + v.Y + v.Y * v.Y + key)) % 2 == 0;
    private static readonly Vec2I[] Neighbours = new[]
    {
        new Vec2I(0, -1),
        new Vec2I(0, 1),
        new Vec2I(-1, 0),
        new Vec2I(1, 0)
    };
    private static Func<Vec2I, IEnumerable<Vec2I>> NeighboursOf(Func<Vec2I, bool> isSpace) => v => Neighbours.Select(x => v + x).Where(isSpace);

    [TestCase("10, 7, 4", 11)]
    public static int ExecutePart1(string input)
    {
        var inputs = input.Split(",").Select(x => x.Trim()).ToArray();
        var target = new Vec2I(int.Parse(inputs[1]), int.Parse(inputs[2]));
        return Utilities.AStarLength(
            new Vec2I(1, 1),
            v => (int)Math.Ceiling(Vec2I.Distance(v, target)), v => v == target,
            NeighboursOf(IsSpace(int.Parse(inputs[0]))))!.Value;
    }
    public static int ExecutePart2(string input)
    {
        var neighboursOf = NeighboursOf(IsSpace(int.Parse(input)));
        var unique = new HashSet<Vec2I>();
        var unvisisted = new PriorityQueue<Vec2I, int>();
        unvisisted.Enqueue(new Vec2I(1, 1), 0);
        unique.Add(unvisisted.Peek());
        while (unvisisted.TryDequeue(out var current, out var currentDistance))
        {
            foreach (var n in neighboursOf(current))
            {
                if (unique.Add(n) && currentDistance + 1 < 50)
                    unvisisted.Enqueue(n, currentDistance + 1);
            }
        }
        return unique.Count;
    }
}
