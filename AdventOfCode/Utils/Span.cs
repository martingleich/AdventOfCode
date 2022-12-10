using System;

namespace AdventOfCode.Utils
{
    public readonly struct Span
    {
        public readonly string Input;
        public readonly int Cursor;
        public readonly int Length;

        private Span(string input, int cursor, int length)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Cursor = cursor;
            Length = length;
        }

        public static Span FromString(string input) => new(input, 0, input.Length);
        public Span Advance(int count) => new(Input, Cursor + count, Length - count);
        public char this[int c] => Input[Cursor + c];
        public Span Substring(int start, int end) => new(Input, Cursor + start, end - start);
        public bool StartsWith(string str)
        {
            if (Length < str.Length)
                return false;
            for (int i = 0; i < str.Length; ++i)
                if (str[i] != Input[Cursor + i])
                    return false;
            return true;
        }
        public override string ToString() => Input.Substring(Cursor, Length);
    }
}
