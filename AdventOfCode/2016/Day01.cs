using AdventOfCode.Utils;
using ProblemsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode._2016
{
    [Problem("2016-01-01", MethodName = nameof(ExecutePart1))]
    [Problem("2016-01-02", MethodName = nameof(ExecutePart2))]
    public class Day01
    {
        private record struct Direction(int X, int Y)
        {
            public Position Move(Position position, int count) => new(position.X + X * count, position.Y + Y * count);
            public Direction Rotate(Direction dir) => new(X * dir.X - Y * dir.Y, X * dir.Y + Y * dir.X);
            public static readonly Direction Forward = new(1, 0);
            public static readonly Direction Backward = new(-1, 0);
            public static readonly Direction Left = new(0, -1);
            public static readonly Direction Right = new(0, 1);
        }
        private record struct Position(int X, int Y)
        {
            public static readonly Position Origin = default;
            public int Length => Math.Abs(X) + Math.Abs(Y);
        }
        private record struct Step(Direction Direction, int Distance);
        private static readonly Parser<Step> StepParser = Parser.MakeRegexParser(@"\s*(R|L)(\d+)\s*", m =>
            new Step(
                m.Groups[1].Value == "L" ? Direction.Left : Direction.Right,
                int.Parse(m.Groups[2].ValueSpan))
        );
        private static readonly Func<string, IEnumerable<Step>> InputParser = input => input.Split(',').Select(StepParser.Parse);
        private record struct State(Direction Dir, Position Pos)
        {
            public State Move(Step step)
            {
                var newDir = Dir.Rotate(step.Direction);
                return new(newDir, newDir.Move(Pos, step.Distance));
            }
            public IEnumerable<State> MoveStepwise(Step step)
            {
                var newDir = Dir.Rotate(step.Direction);
                var newPos = Pos;
                for (int i = 0; i < step.Distance; ++i)
                {
                    newPos = newDir.Move(newPos, 1);
                    yield return new(newDir, newPos);
                }
            }
        }
        [TestCase("R2, L3", 5)]
        [TestCase("R2, R2, R2", 2)]
        [TestCase("R5, L5, R5, R3", 12)]
        public static int ExecutePart1(string input)
        {
            var pos = InputParser(input).Aggregate(new State(Direction.Forward, Position.Origin), (state, step) => state.Move(step)).Pos;
            return pos.Length;
        }

        [TestCase("R8, R4, R4, R8", 4)]
        public static int ExecutePart2(string input)
        {
            var visisted = new HashSet<Position>() { Position.Origin };
            var state = new State(Direction.Forward, Position.Origin);
            foreach (var step in InputParser(input))
            {
                foreach (var stepState in state.MoveStepwise(step))
                {
                    if (!visisted.Add(stepState.Pos))
                        return stepState.Pos.Length;
                    state = stepState;
                }
            }
            throw new InvalidOperationException("No position is visisted twice");
        }
    }
}
