using AdventOfCode.Utils;
using ProblemsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode._2016
{
    [Problem("2016-02-01", MethodName = nameof(ExecutePart1))]
    [Problem("2016-02-02", MethodName = nameof(ExecutePart2))]
    public class Day2
    {
        private static string Execute(string input, string numpad)
        {
            var digit = Digit.Read(numpad);
            var result = "";
            foreach (var line in input.SplitLines())
            {
                foreach (var c in line)
                    digit = digit.Update(c);
                result += digit.Value;
            }
            return result;
        }
        [TestCase(@"
ULL
RRDDD
LURDL
UUUUD
", "1985")]
        public static string ExecutePart1(string input) => Execute(input, @"
1 2 3
4 5 6
7 8 9");


        [TestCase(@"
ULL
RRDDD
LURDL
UUUUD
", "5DB3")]
        public static string ExecutePart2(string input) => Execute(input, @"
. . 1 . .
. 2 3 4 .
5 6 7 8 9
. A B C .
. . D . .");
        private class Digit
        {
            private Digit? Up;
            private Digit? Down;
            private Digit? Left;
            private Digit? Right;
            public char Value { get; private set; }

            public Digit Update(char c) => c switch
            {
                'U' => Up,
                'D' => Down,
                'L' => Left,
                'R' => Right,
                _ => this,
            } ?? this;

            public static Digit Read(string numpad, char startValue = '5')
            {
                var digits = new Dictionary<(int, int), Digit>();
                var pos = (0, 0);
                var start = default(Digit?);
                foreach (var line in numpad.SplitLines())
                {
                    foreach (var n in line.Where(c => char.IsLetterOrDigit(c) || c == '.'))
                    {
                        if (n != '.')
                        {
                            var digit = new Digit() { Value = n };
                            digits.Add(pos, digit);
                            if (n == startValue)
                                start = digit;
                        }
                        pos.Item1++;
                    }
                    pos.Item2++;
                    pos.Item1 = 0;
                }
                foreach (var (p, digit) in digits)
                {
                    digit.Up = digits.GetValueOrDefault((p.Item1, p.Item2 - 1));
                    digit.Down = digits.GetValueOrDefault((p.Item1, p.Item2 + 1));
                    digit.Left = digits.GetValueOrDefault((p.Item1 - 1, p.Item2));
                    digit.Right = digits.GetValueOrDefault((p.Item1 + 1, p.Item2));
                }
                return start ?? throw new ArgumentException("No start value on the numpad");
            }
            public override string ToString() => Value.ToString();
        }
    }
}
