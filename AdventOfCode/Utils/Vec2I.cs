using System;

namespace AdventOfCode.Utils;

    public readonly record struct Vec2I(int X, int Y)
    {
        public static readonly Vec2I Right = new(1, 0);
        public static readonly Vec2I Left = new(-1, 0);
        public static readonly Vec2I Up = new(0, 1);
        public static readonly Vec2I Down = new(0, -1);

        public static int TaxiCabDistance(Vec2I a, Vec2I b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        public static Vec2I operator +(Vec2I a, Vec2I b) => new (a.X + b.X, a.Y + b.Y);
    }
