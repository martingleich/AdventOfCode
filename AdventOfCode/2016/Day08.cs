using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using AdventOfCode.Utils;
using ProblemsLibrary;
// ReSharper disable StringLiteralTypo

namespace AdventOfCode._2016;

[Problem("2016-08-01", MethodName = nameof(ExecutePart1))]
[Problem("2016-08-02", MethodName = nameof(ExecutePart2))]
public class Day08
{
    private abstract record Command
    {
        [Pure]
        public abstract Matrix<bool> Update(Matrix<bool> matrix);
    }
    private sealed record RectCommand(int Width, int Height) : Command
    {
        public override Matrix<bool> Update(Matrix<bool> matrix)
            => matrix.SetSubMatrix(0, 0, Matrix.Fill(Width, Height, true));
    }

    private sealed record RotateRowCommand(int Row, int Shift) : Command
    {
        public override Matrix<bool> Update(Matrix<bool> matrix)
            => matrix.MapRow(Row, v => v.Rotate(Shift));
    }

    private sealed record RotateColumnCommand(int Column, int Shift) : Command
    {
        public override Matrix<bool> Update(Matrix<bool> matrix)
            => matrix.MapColumn(Column, v => v.Rotate(Shift));
    }

    public static Matrix<bool> ExecuteCommon(string input, int width, int height)
    {
        var parseRect = Parser.MakeRegexParser(new Regex(@"rect (\d+)x(\d+)"), m => (Command)new RectCommand(int.Parse(m.Groups[1].ValueSpan), int.Parse(m.Groups[2].ValueSpan)));
        var parseRotateRow = Parser.MakeRegexParser(new Regex(@"rotate row y=(\d+) by (\d+)"), m => (Command)new RotateRowCommand(int.Parse(m.Groups[1].ValueSpan), int.Parse(m.Groups[2].ValueSpan)));
        var parseRotateColumn = Parser.MakeRegexParser(new Regex(@"rotate column x=(\d+) by (\d+)"), m => (Command)new RotateColumnCommand(int.Parse(m.Groups[1].ValueSpan), int.Parse(m.Groups[2].ValueSpan)));
        var parseLine = Parser.OneOf(parseRect, parseRotateRow, parseRotateColumn);
        var commands = input.SplitLines().Select(parseLine.Parse).ToArray();
        var matrix = Matrix.Default<bool>(width, height);
        foreach (var cmd in commands)
            matrix = cmd.Update(matrix);
        return matrix;
    }
    [TestCase(@"
rect 3x2
rotate column x=1 by 1
rotate row y=0 by 4
rotate column x=1 by 1
", 7, 3, 6)
        ]
    public static int ExecutePart1_Testable(string input, int width, int height) => ExecuteCommon(input, width, height).GetValues().Count(x => x);
    public static int ExecutePart1(string input) => ExecutePart1_Testable(input, 50, 6);
    public static string ExecutePart2(string input)
    {
        var matrix = ExecuteCommon(input, 50, 6);
        return Environment.NewLine + string.Join(Environment.NewLine,
            matrix.GetRows().Select(r => new string(r.GetValues().Select(x => x ? '#' : ' ').ToArray()))) + Environment.NewLine;
    }
}
