using System;
using System.Diagnostics.CodeAnalysis;

namespace AdventOfCode.Utils
{
    public readonly struct Result<T>
    {
        private readonly T _value;

        private Result(T value, bool okay)
        {
            _value = value;
            HasValue = okay;
        }
        public static Result<T> Okay(T value) => new(value, true);
        public static readonly Result<T> Error = default;

        [MemberNotNullWhen(true, "Value")]
        public bool HasValue { get; }

        public T Value => HasValue ? _value : throw new InvalidOperationException();
        public bool TryGetValue([NotNullWhen(true), MaybeNull] out T value)
        {
            value = HasValue ? _value : default;
            return HasValue;
        }

        public override string? ToString() => TryGetValue(out var value) ? value.ToString() : "null";
        public Result<TResult> Map<TResult>(Func<T, TResult> map) => TryGetValue(out var value)
            ? Result.Okay(map(value))
            : Result<TResult>.Error;
    }
}
