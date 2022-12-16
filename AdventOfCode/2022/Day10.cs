using System;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AdventOfCode._2022;

[Problem("2022-10-01", MethodName = nameof(ExecutePart1))]
[Problem("2022-10-02", MethodName = nameof(ExecutePart2))]
public class Day10
{
    private const string TestData = @"
addx 15
addx -11
addx 6
addx -3
addx 5
addx -1
addx -8
addx 13
addx 4
noop
addx -1
addx 5
addx -1
addx 5
addx -1
addx 5
addx -1
addx 5
addx -1
addx -35
addx 1
addx 24
addx -19
addx 1
addx 16
addx -11
noop
noop
addx 21
addx -15
noop
noop
addx -3
addx 9
addx 1
addx -3
addx 8
addx 1
addx 5
noop
noop
noop
noop
noop
addx -36
noop
addx 1
addx 7
noop
noop
noop
addx 2
addx 6
noop
noop
noop
noop
noop
addx 1
noop
noop
addx 7
addx 1
noop
addx -13
addx 13
addx 7
noop
addx 1
addx -33
noop
noop
noop
addx 2
noop
noop
noop
addx 8
noop
addx -1
addx 2
addx 1
noop
addx 17
addx -9
addx 1
addx 1
addx -3
addx 11
noop
noop
addx 1
noop
addx 1
noop
noop
addx -13
addx -19
addx 1
addx 3
addx 26
addx -30
addx 12
addx -1
addx 3
addx 1
noop
noop
noop
addx -9
addx 18
addx 1
addx 2
noop
noop
addx 9
noop
noop
noop
addx -1
addx 2
addx -37
addx 1
addx 3
noop
addx 15
addx -21
addx 22
addx -6
addx 1
noop
addx 2
addx 1
noop
addx -10
noop
noop
addx 20
addx 1
addx 2
addx 2
addx -6
addx -11
noop
noop
noop";

    public static void Processor(string input, Action<int, int> onCycle)
    {
        var parser = Parser.OneOf(
            Parser.FormattedString($"addx {Parser.SignedInteger}", m => new Instruction(2, m[0])),
            Parser.Fixed("noop").Return(new Instruction(1, 0)));
        var register = 1;
        var cycle = 0;
        foreach (var instruction in parser.ParseValidLines(input))
        {
            for (var i = 0; i < instruction.Cycles; ++i)
            {
                ++cycle;
                onCycle(cycle, register);
            }

            register += instruction.Increment;
        }
    }

    [TestCase(TestData, 13140)]
    public static int ExecutePart1(string input)
    {
        var result = 0;
        Processor(input, (cycle, register) =>
        {
            if (cycle >= 20 && (cycle - 20) % 40 == 0)
                result += register * cycle;
        });
        return result;
    }

    public static string ExecutePart2(string input)
    {
        var pixels = Enumerable.Range(0, 6).Select(_ => new char[40]).ToArray();
        Processor(input, (cycle, register) =>
        {
            var row = (cycle - 1) % 40;
            pixels[(cycle - 1) / 40][row] = Math.Abs(row - register) <= 1 ? '#' : '.';
        });
        return Environment.NewLine + string.Join(Environment.NewLine, pixels.Select(p => new string(p)));
    }

    private readonly record struct Instruction(int Cycles, int Increment);
}