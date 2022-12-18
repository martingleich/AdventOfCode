using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AdventOfCode._2022;

[Problem("2022-09-01", MethodName = nameof(ExecutePart1))]
[Problem("2022-09-02", MethodName = nameof(ExecutePart2))]
public class Day09
{
    private static IEnumerable<Vec2I> ParseInput(string input)
    {
        var parserDirection = Parser.OneOf(
            Parser.Fixed("L").Return(Vec2I.Left),
            Parser.Fixed("R").Return(Vec2I.Right),
            Parser.Fixed("U").Return(Vec2I.Up),
            Parser.Fixed("D").Return(Vec2I.Down));
        var parserCommands = Parser.FormattedString($"{parserDirection} {Parser.UnsignedInteger}",
            m => Enumerable.Repeat((Vec2I)m[0], (int)m[1]));
        return input.SplitLines().SelectMany(parserCommands.Parse);
    }

    [TestCase(@"R 4
U 4
L 3
D 1
R 4
D 1
L 5
R 2", 13)]
    public static int ExecutePart1(string input)
    {
        return CommonExecute(input, 2);
    }

    [TestCase(@"R 5
U 8
L 8
D 3
R 17
D 10
L 25
U 20", 36)]
    public static int ExecutePart2(string input)
    {
        return CommonExecute(input, 10);
    }

    private static int CommonExecute(string input, int length)
    {
        return ParseInput(input)
            .RunningAggregate(Snake.Create(length, default), (snake, step) => snake.Move(step))
            .DistinctBy(r => r.TailPosition)
            .Count();
    }

    private abstract record Snake(Vec2I Position)
    {
        public abstract Vec2I TailPosition { get; }

        public Snake Move(Vec2I step)
        {
            return SetPosition(Position + step);
        }

        protected abstract Snake SetPosition(Vec2I newHead);

        public static Snake Create(int length, Vec2I position)
        {
            if (length == 1)
                return new TailSnake(position);
            return new InnerSnake(position, Create(length - 1, position));
        }

        protected sealed record TailSnake(Vec2I Position) : Snake(Position)
        {
            public override Vec2I TailPosition => Position;

            protected override TailSnake SetPosition(Vec2I newHead)
            {
                return new(newHead);
            }
        }

        protected sealed record InnerSnake(Vec2I Position, Snake Tail) : Snake(Position)
        {
            public override Vec2I TailPosition => Tail.TailPosition;

            protected override InnerSnake SetPosition(Vec2I newHead)
            {
                var dx = newHead.X - Tail.Position.X;
                var dy = newHead.Y - Tail.Position.Y;
                if (Math.Max(Math.Abs(dx), Math.Abs(dy)) <= 1)
                    return new InnerSnake(newHead, Tail);

                Vec2I tailMove;
                if (dx == 0 || dy == 0 || Math.Abs(dx) == Math.Abs(dy))
                    tailMove = new Vec2I(dx / 2, dy / 2);
                else if (Math.Abs(dx) > Math.Abs(dy))
                    tailMove = new Vec2I(dx / 2, dy);
                else
                    tailMove = new Vec2I(dx, dy / 2);
                return new InnerSnake(newHead, Tail.SetPosition(Tail.Position + tailMove));
            }
        }
    }
}