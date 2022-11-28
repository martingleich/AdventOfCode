using ProblemsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode._2016
{
    [Problem("2016-01-01", MethodName =nameof(ExecutePart1))]
    [Problem("2016-01-02", MethodName =nameof(ExecutePart2))]
    public class Day1
    {
        private enum Direction
        {
            North,
            East,
            South,
            West,
        }
        private static Direction Rotate(Direction dir, int steps) => (Direction)(((int)dir + steps) % 4);
        private record struct Position(int X, int Y)
        {
            public int Length => Math.Abs(X) + Math.Abs(Y);
            public Position Steps(Direction dir, int count) => dir switch
            {
                Direction.North => this with { Y = Y + count },
                Direction.South => this with { Y = Y - count },
                Direction.East => this with { X = X + count },
                Direction.West => this with { X = X - count },
                _ => throw new NotImplementedException(),
            };
        }
        private record struct Step (int ClockwiseSteps, int Distance);
        private static readonly Func<string, Step?> StepParser = Utilities.MakeRegexParserStruct(@"(R|L)(\d+)", m =>
            new Step(
                m.Groups[1].Value == "L" ? 3 : 1,
                int.Parse(m.Groups[2].ValueSpan))
        );
        private static readonly Func<string, IEnumerable<Step>> InputParser = input => input.Split(',').Select(x => StepParser(x.Trim())!.Value);
        private record struct State(Direction Dir, Position Pos)
        {
            public State Move(Step step)
            {
                var newDir = Rotate(Dir, step.ClockwiseSteps);
                return new(newDir, Pos.Steps(newDir, step.Distance));
            }
            public IEnumerable<State> MoveStepwise(Step step)
            {
                var newDir = Rotate(Dir, step.ClockwiseSteps);
                var newPos = Pos;
                for(int i = 0; i < step.Distance; ++i)
                {
                    newPos = newPos.Steps(newDir, 1);
                    yield return new(newDir, newPos);
                }
            }
        }
        [TestCase("R2, L3", 5)]
        [TestCase("R2, R2, R2", 2)]
        [TestCase("R5, L5, R5, R3", 12)]
        public static int ExecutePart1(string input)
        {
            var pos = InputParser(input).Aggregate(new State(default, default), (state, step) => state.Move(step)).Pos;
            return pos.Length;
        }
        
        [TestCase("R8, R4, R4, R8", 4)]
        public static int ExecutePart2(string input)
        {
            var visisted = new HashSet<Position>() { default };
            var state = new State(default, default);
            foreach (var step in InputParser(input))
            {
                foreach(var stepState in state.MoveStepwise(step))
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
