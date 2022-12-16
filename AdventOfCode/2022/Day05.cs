using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AdventOfCode._2022;

[Problem("2022-05-01", MethodName = nameof(ExecutePart1))]
[Problem("2022-05-02", MethodName = nameof(ExecutePart2))]
public class Day05
{
    private const string TestData = @"
    [D]    
[N] [C]    
[Z] [M] [P]
 1   2   3 

move 1 from 2 to 1
move 3 from 1 to 3
move 2 from 2 to 1
move 1 from 1 to 2";

    [TestCase(TestData, "CMZ")]
    public static string ExecutePart1(string input)
    {
        return ExecuteCommon(input, Command.Single);
    }

    [TestCase(TestData, "MCD")]
    public static string ExecutePart2(string input)
    {
        return ExecuteCommon(input, Command.Multiple);
    }

    private static string ExecuteCommon(string input, Func<int, int, int, Command> commandFactory)
    {
        var parts = input.SplitLines(Utilities.SplitLineOptions.None)
            .Split(string.IsNullOrEmpty).ToArray();
        var parserTableEntry = Parser.OneOf(
            Parser.Fixed("   ").Return(default(char?)),
            Parser.MakeRegexParser(new Regex(@"^\[.\]"), m => (char?)m.ValueSpan[1]));
        var parserTableLine = parserTableEntry.DelimitedWith(Parser.Fixed(" "));
        var tableValues = parts[0].Select(parserTableLine.TryParse).WhereOkay();
        var stacks = new Dictionary<int, Stack<char>>();
        foreach (var tableLine in tableValues.Reverse())
        {
            var colId = 1;
            foreach (var item in tableLine)
            {
                if (!stacks.TryGetValue(colId, out var column))
                    stacks[colId] = column = new Stack<char>();
                if (item.HasValue)
                    column.Push(item.Value);

                ++colId;
            }
        }

        var parserCommand = Parser.FormattedString(
            $"move {Parser.UnsignedInteger} from {Parser.UnsignedInteger} to {Parser.UnsignedInteger}",
            m => commandFactory(m[0], m[1], m[2]));
        var commands = parts[1].Select(parserCommand.Parse);
        foreach (var command in commands)
            command.Perform(stacks);

        return new string(stacks.OrderBy(x => x.Key).Select(x => x.Value.Peek()).ToArray());
    }

    public abstract record Command(int Count, int From, int To)
    {
        public static Command Single(int count, int from, int to)
        {
            return new CommandSingle(count, from, to);
        }

        public static Command Multiple(int count, int from, int to)
        {
            return new CommandMultiple(count, from, to);
        }

        public abstract void Perform(IReadOnlyDictionary<int, Stack<char>> stacks);

        private sealed record CommandSingle(int Count, int From, int To) : Command(Count, From, To)
        {
            public override void Perform(IReadOnlyDictionary<int, Stack<char>> stacks)
            {
                for (var i = 0; i < Count; ++i)
                    stacks[To].Push(stacks[From].Pop());
            }
        }

        private sealed record CommandMultiple(int Count, int From, int To) : Command(Count, From, To)
        {
            public override void Perform(IReadOnlyDictionary<int, Stack<char>> stacks)
            {
                var tmp = new Stack<char>();
                for (var i = 0; i < Count; ++i)
                    tmp.Push(stacks[From].Pop());
                for (var i = 0; i < Count; ++i)
                    stacks[To].Push(tmp.Pop());
            }
        }
    }
}