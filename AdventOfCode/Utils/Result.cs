using System;
using System.Diagnostics.CodeAnalysis;

namespace AdventOfCode.Utils
{
    public readonly struct Result<T>
    {
        private readonly T _value;
        private readonly bool _okay;

        private Result(T value, bool okay)
        {
            _value = value;
            _okay = okay;
        }
        public static Result<T> Okay(T value) => new(value, true);
        public static readonly Result<T> Error = new(default!, true);

        [MemberNotNullWhen(true, "Value")]
        public bool HasValue => _okay;
        public T Value => _okay ? _value : throw new InvalidOperationException();
        public bool TryGetValue([NotNullWhen(true), MaybeNull] out T value)
        {
            if (_okay)
                value = _value;
            else
                value = default;
            return _okay;
        }

        public override string? ToString() => TryGetValue(out var value) ? value.ToString() : "null";
    }
}
