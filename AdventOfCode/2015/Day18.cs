using ProblemsLibrary;
using System.Linq;

namespace AventOfCode._2015
{
    public class Day18_Common
    {
        public static int Execute(string input, int count, bool fixCorners)
        {
            var grid = MapInput(input);
            return Execute(grid, count, fixCorners);
        }

        private static int Execute(bool[,] initial, int count, bool fixCorners)
        {
            if (fixCorners)
            {
                initial[0, 0] = true;
                initial[0, initial.GetLength(1) - 1] = true;
                initial[initial.GetLength(0) - 1, 0] = true;
                initial[initial.GetLength(0) - 1, initial.GetLength(1) - 1] = true;
            }
            var result = new bool[initial.GetLength(0), initial.GetLength(1)];
            for (int c = 0; c < count; ++c)
            {
                for (int i = 0; i < initial.GetLength(0); ++i)
                    for (int j = 0; j < initial.GetLength(1); ++j)
                    {
                        int countNeighbours = GetNeighbourCount(initial, i, j);
                        if (Get(initial, i, j) == 1)
                            result[i, j] = countNeighbours == 2 || countNeighbours == 3;
                        else
                            result[i, j] = countNeighbours == 3;
                    }

                if (fixCorners)
                {
                    result[0, 0] = true;
                    result[0, initial.GetLength(1) - 1] = true;
                    result[initial.GetLength(0) - 1, 0] = true;
                    result[initial.GetLength(0) - 1, initial.GetLength(1) - 1] = true;
                }
                (initial, result) = (result, initial);
            }
            int alive = 0;
            for (int i = 0; i < initial.GetLength(0); ++i)
                for (int j = 0; j < initial.GetLength(1); ++j)
                    alive += Get(initial, i, j);
            return alive;

            static int GetNeighbourCount(bool[,] initial, int i, int j) =>
                Get(initial, i - 1, j)
                + Get(initial, i - 1, j - 1)
                + Get(initial, i - 1, j + 1)
                + Get(initial, i, j + 1)
                + Get(initial, i, j - 1)
                + Get(initial, i + 1, j)
                + Get(initial, i + 1, j - 1)
                + Get(initial, i + 1, j + 1);
            static byte Get(bool[,] arr, int i, int j)
            {
                if (i < 0 || j < 0)
                    return 0;
                else if (i >= arr.GetLength(0) || j >= arr.GetLength(1))
                    return 0;
                else
                    return arr[i, j] ? (byte)1 : (byte)0;
            }
        }

        private static bool[,] MapInput(string input)
        {
            static bool? MapChar(char c)
            {
                if (c == '.')
                    return false;
                if (c == '#')
                    return true;
                return null;
            }
            var values = input.Select(MapChar).WhereStructNotNull().ToSquareArray();
            return values;
        }
    }

    [Problem("2015-18-01")]
    public class Day18_Part1
    {
        [TestCase(@"
.#.#.#
...##.
#....#
..#...
#.#..#
####..",
            4, 4)]
        public static int Execute(string input, int count) => Day18_Common.Execute(input, count, false);
        public int Execute(string input) => Execute(input, 100);
    }

    [Problem("2015-18-02")]
    public class Day18_Part2
    {
        [TestCase(@"
.#.#.#
...##.
#....#
..#...
#.#..#
####..",
            5, 17)]
        public static int Execute(string input, int count) => Day18_Common.Execute(input, count, true);
        public int Execute(string input) => Execute(input, 100);
    }
}
