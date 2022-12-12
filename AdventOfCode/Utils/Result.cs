using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;

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

    public static class ResultEx
    {
        public static Result<T> MapExtractPartialParsed<T>(this Result<PartialParsed<T>> self) =>
            self.TryGetValue(out var partialParsed) ? Result.Okay(partialParsed.Value) : default;
        public static Result<PartialParsed<TResult>> MapPartialParsed<T, TResult>(this Result<PartialParsed<T>> self, Func<T, TResult> map) => self.TryGetValue(out var value)
            ? Result.Okay(PartialParsed.Create(map(value.Value), value.Remaining))
            : default;
        public static Result<PartialParsed<object?>> CastToObject<T>(this Result<PartialParsed<T>> self) => self.TryGetValue(out var value)
            ? Result.Okay(PartialParsed.Create((object?)value.Value, value.Remaining))
            : default;
    }
}
