using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AdventOfCode._2022;

[Problem("2022-12-01", MethodName = nameof(ExecutePart1))]
[Problem("2022-12-02", MethodName = nameof(ExecutePart2))]
public class Day12
{
    private readonly record struct Grid(Matrix<byte> Heightmap)
    {
        private static readonly ImmutableArray<Vec2I> Offsets = ImmutableArray.Create(
            new Vec2I(-1, 0),
            new Vec2I(1, 0),
            new Vec2I(0, -1),
            new Vec2I(0, 1));
        public IEnumerable<Vec2I> Neighbours(Vec2I current)
        {
            foreach (var off in Offsets)
                if (GetNeighbour(current, current + off) is Vec2I p)
                    yield return p;
        }
        private Vec2I? GetNeighbour(Vec2I current, Vec2I pos)
        {
            if (pos.X < 0 || pos.X >= Heightmap.NumColumns)
                return null;
            if (pos.Y < 0 || pos.Y >= Heightmap.NumRows)
                return null;
            if (Heightmap[current.X, current.Y] <= Heightmap[pos.X, pos.Y] + 1)
                return pos;
            else
                return null;
        }
    }
    private static (Grid, Vec2I, Vec2I) ReadInput(string input)
    {
        var inputGrid = Matrix.FromRows(Parser.Repeat(Parser.Char).ThenIgnore(Parser.NewLine).ParseRepeated(input));
        var grid = inputGrid.MapValues(c => c switch
            {
                'S' => Decode('a'),
                'E' => Decode('z'),
                char x => Decode(x)
            });

        var start = inputGrid.AllWhere(c => c == 'S').Single();
        var end = inputGrid.AllWhere(c => c == 'E').Single();
        return (new Grid(grid), start, end);
        static byte Decode(char c) => (byte)(c - 'a');
    }
   [TestCase(@"Sabqponm
abcryxxl
accszExk
acctuvwj
abdefghi
", 31)]
    public static int ExecutePart1(string input)
    {
        var (grid, start, end) = ReadInput(input);
        return FindAllPaths(end, grid)[start];
    }
    private static Dictionary<Vec2I, int> FindAllPaths(Vec2I end, Grid grid)
    {
        var dist = new Dictionary<Vec2I, int>() { [end] = 0 };
        var q = new PriorityQueue<Vec2I, int>();
        foreach (var v in grid.Heightmap.GetIndexed())
        {
            if(v.Key != end)
                dist[v.Key] = int.MaxValue;
            q.Enqueue(v.Key, dist[v.Key]);
        }
        while(q.TryDequeue(out var u, out var p))
        {
            if(p == dist[u])
            {
                foreach(var v in grid.Neighbours(u))
                {
                    var alt = dist[u] == int.MaxValue ? int.MaxValue : dist[u] + 1;
                    if(alt < dist[v])
                    {
                        dist[v] = alt;
                        q.Enqueue(v, alt);
                    }
                }
            }
        }
        return dist;
    }

   [TestCase(@"Sabqponm
abcryxxl
accszExk
acctuvwj
abdefghi
", 29)]
    public static int ExecutePart2(string input)
    {
        var (grid, start, end) = ReadInput(input);
        var allStarts = grid.Heightmap.AllWhere(h => h == 0).ToImmutableArray();
        var allPaths = FindAllPaths(end, grid);
        return allStarts.Select(x => allPaths[x]).Min();
    }
}