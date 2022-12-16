using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;

// ReSharper disable StringLiteralTypo

namespace AdventOfCode._2016;

[Problem("2016-10-01", MethodName = nameof(ExecutePart1))]
[Problem("2016-10-02", MethodName = nameof(ExecutePart2))]
public class Day10
{
    public static (int, int) ExecutePartCommon(string input)
    {
        var parserDestinationBot =
            Parser.FormattedString($"bot {Parser.UnsignedInteger}", m => (Destination)new BotDestination(m[0]));
        var parserDestinationOutput = Parser.FormattedString($"output {Parser.UnsignedInteger}",
            m => (Destination)new OutputDestination(m[0]));
        var parserDestination = Parser.OneOf(parserDestinationBot, parserDestinationOutput);
        var parserCompare =
            Parser.FormattedString(
                $"bot {Parser.UnsignedInteger} gives low to {parserDestination} and high to {parserDestination}",
                m => (IAction)new CompareAction(m[0], m[1], m[2]));
        var parserGive = Parser.FormattedString($"value {Parser.UnsignedInteger} goes to bot {Parser.UnsignedInteger}",
            m => (IAction)new GiveAction(m[1], m[0]));
        var parserLine = Parser.OneOf(parserCompare, parserGive);

        var actions = input.SplitLines().Select(parserLine.Parse).ToHashSet();
        var allBots = new Dictionary<int, Bot>();

        Bot getBot(int i)
        {
            if (!allBots.TryGetValue(i, out var bot))
                allBots[i] = bot = new Bot();
            return bot;
        }

        var allOutputs = new Dictionary<int, Output>();

        Output getOutput(int i)
        {
            if (!allOutputs.TryGetValue(i, out var output))
                allOutputs[i] = output = new Output();
            return output;
        }

        var done = new HashSet<IAction>();
        var resultPart1 = 0;
        while (actions.Count > 0)
        {
            foreach (var action in actions)
                if (action.TryPerform(getBot, getOutput))
                {
                    if (action is CompareAction ca && getBot(ca.Id).GetMinMax() is (int min, int max) && min == 17 &&
                        max == 61)
                        resultPart1 = ca.Id;
                    done.Add(action);
                }

            foreach (var d in done)
                actions.Remove(d);
            done.Clear();
        }

        return (resultPart1, getOutput(0).Value * getOutput(1).Value * getOutput(2).Value);
    }

    public static int ExecutePart1(string input)
    {
        return ExecutePartCommon(input).Item1;
    }

    public static int ExecutePart2(string input)
    {
        return ExecutePartCommon(input).Item2;
    }

    private abstract record class Destination
    {
        public abstract IReceiver GetReceiver(Func<int, Bot> bots, Func<int, Output> outputs);
    }

    private sealed record class BotDestination(int id) : Destination
    {
        public override IReceiver GetReceiver(Func<int, Bot> bots, Func<int, Output> outputs)
        {
            return bots(id);
        }
    }

    private sealed record class OutputDestination(int id) : Destination
    {
        public override IReceiver GetReceiver(Func<int, Bot> bots, Func<int, Output> outputs)
        {
            return outputs(id);
        }
    }

    private interface IAction
    {
        public bool TryPerform(Func<int, Bot> bots, Func<int, Output> outputs);
    }

    private sealed record class GiveAction(int Id, int Value) : IAction
    {
        public bool TryPerform(Func<int, Bot> bots, Func<int, Output> outputs)
        {
            bots(Id).Receive(Value);
            return true;
        }
    }

    private sealed record class CompareAction(int Id, Destination Low, Destination High) : IAction
    {
        public bool TryPerform(Func<int, Bot> bots, Func<int, Output> outputs)
        {
            var self = bots(Id);
            if (self.GetMinMax() is (int min, int max))
            {
                Low.GetReceiver(bots, outputs).Receive(min);
                High.GetReceiver(bots, outputs).Receive(max);
                return true;
            }

            return false;
        }
    }

    public interface IReceiver
    {
        void Receive(int value);
    }

    private class Bot : IReceiver
    {
        private int _count;
        private int _maxValue;
        private int _minValue;

        public void Receive(int value)
        {
            if (_count == 0)
            {
                _minValue = value;
                ++_count;
            }
            else if (_count == 1)
            {
                var oldValue = _minValue;
                _minValue = Math.Min(value, oldValue);
                _maxValue = Math.Max(value, oldValue);
                ++_count;
            }
        }

        public (int, int)? GetMinMax()
        {
            return _count >= 2 ? ((int, int)?)(_minValue, _maxValue) : default;
        }
    }

    private class Output : IReceiver
    {
        public int Value { get; set; }

        public void Receive(int value)
        {
            Value = value;
        }
    }
}