using System;
using System.Linq;
using System.Text.RegularExpressions;
using AdventOfCode.Utils;
using ProblemsLibrary;
// ReSharper disable StringLiteralTypo

namespace AdventOfCode._2016;

[Problem("2016-08-01", MethodName = nameof(ExecutePart1))]
[Problem("2016-08-02", MethodName = nameof(ExecutePart2))]
public class Day8
{
    private abstract record class Command
    {
        public abstract void Update(Matrix<bool> matrix);
    }
    private sealed record class RectCommand(int Width, int Height) : Command
    {
        public override void Update(Matrix<bool> matrix)
        {
            for (int r = 0; r < Height; ++r)
                for (int c = 0; c < Width; ++c)
                    matrix[c, r] = true;
        }
    }

    private sealed record class RotateRowCommand(int Row, int Shift) : Command
    {
        public override void Update(Matrix<bool> matrix)
        {
            matrix.RotateRow(Row, Shift);
        }
    }

    private sealed record class RotateColumnCommand(int Column, int Shift) : Command
    {
        public override void Update(Matrix<bool> matrix)
        {
            matrix.RotateColumn(Column, Shift);
        }
    }

    public static Matrix<bool> ExecuteCommon(string input, int width, int height)
    {
        var parseRect = Parser.MakeRegexParser(new Regex(@"rect (\d+)x(\d+)"), m => (Command)new RectCommand(int.Parse(m.Groups[1].ValueSpan), int.Parse(m.Groups[2].ValueSpan)));
        var parseRotateRow = Parser.MakeRegexParser(new Regex(@"rotate row y=(\d+) by (\d+)"), m => (Command)new RotateRowCommand(int.Parse(m.Groups[1].ValueSpan), int.Parse(m.Groups[2].ValueSpan)));
        var parseRotateColumn = Parser.MakeRegexParser(new Regex(@"rotate column x=(\d+) by (\d+)"), m => (Command)new RotateColumnCommand(int.Parse(m.Groups[1].ValueSpan), int.Parse(m.Groups[2].ValueSpan)));
        var parseLine = Parser.OneOf(parseRect, parseRotateRow, parseRotateColumn);
        var commands = input.SplitLines().Select(parseLine.Parse).ToArray();
        var matrix = Matrix.NewFilled(width, height, false);
        foreach (var cmd in commands)
            cmd.Update(matrix);
        return matrix;
    }
    [TestCase(@"
rect 3x2
rotate column x=1 by 1
rotate row y=0 by 4
rotate column x=1 by 1
", 7, 3, 6)
        ]
    public static int ExecutePart1_Testable(string input, int width, int height) => ExecuteCommon(input, width, height).RowMajor.Count(x => x);
    public static int ExecutePart1(string input) => ExecutePart1_Testable(input, 50, 6);
    public static string ExecutePart2(string input)
    {
        var matrix = ExecuteCommon(input, 50, 6);
        return Environment.NewLine + string.Join(Environment.NewLine, matrix.Rows.Select(r => new string(r.Select(x => x ? '#' : ' ').ToArray()))) + Environment.NewLine;
    }
}
