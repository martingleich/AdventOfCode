using System;
using System.Collections.Immutable;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace AdventOfCode.Utils
{
    public abstract class Parser<T>
    {
        public T Parse(string input) => Parse(Span.FromString(input));
        public T Parse(Span span) => TryParse(span).Value;
        public Result<T> TryParse(string input) => TryParse(Span.FromString(input));
        public Result<T> TryParse(Span span) => ParsePartial(span).Map(v => v.Value);
        public abstract Result<PartialParsed<T>> ParsePartial(Span input);
    }
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
        public override string ToString() => Input.Substring(Cursor, Length);
    }

    public readonly struct PartialParsed<T>
    {
        public readonly T Value;
        public readonly Span Remaining;

        public PartialParsed(T value, Span remaining)
        {
            Value = value;
            Remaining = remaining;
        }

        public PartialParsed<TResult> Map<TResult>(Func<T, TResult> map) => PartialParsed.Create(map(Value), Remaining);
        public PartialParsed<T> Advance(int count) => PartialParsed.Create(Value, Remaining.Advance(count));
    }

    public static class PartialParsed
    {
        public static PartialParsed<T> Create<T>(T value, Span remaining) => new (value, remaining);
    }

    public static class Parser
    {
        public static readonly Parser<string> AlphaNumeric = MakeRegexParser(new Regex(@"[a-zA-Z0-9]+"), m => m.Value);
        public static readonly Parser<int> Int32 = MakeRegexParser(new Regex(@"\d+"), m => int.Parse(m.Value));

        private sealed class RegexParser : Parser<Match>
        {
            private readonly Regex _regex;

            public RegexParser(Regex regex)
            {
                _regex = regex ?? throw new ArgumentNullException(nameof(regex));
            }
            public override Result<PartialParsed<Match>> ParsePartial(Span input)
            {
                var m = _regex.Match(input.Input, input.Cursor, input.Length);
                return m.Success ? Result.Okay(PartialParsed.Create(m, input.Advance(m.Length))) : default;
            }
        }

        public static Parser<Match> ToParser(this Regex regex) => new RegexParser(regex);
        public static Parser<T> MakeRegexParser<T>(string regex, Func<Match, T> converter) => MakeRegexParser(new Regex(regex), converter);
        public static Parser<T> MakeRegexParser<T>(Regex regex, Func<Match, T> converter) => regex.ToParser().Select(converter);

        private sealed class OneOfParser<T> : Parser<T>
        {
            private readonly ImmutableArray<Parser<T>> _alternatives;

            public OneOfParser(ImmutableArray<Parser<T>> alternatives)
            {
                _alternatives = alternatives;
            }

            public override Result<PartialParsed<T>> ParsePartial(Span input)
            {
                foreach (var parser in _alternatives)
                {
                    var result = parser.ParsePartial(input);
                    if (result.HasValue)
                        return result;
                }
                return default;
            }
        }

        public static Parser<T> OneOf<T>(params Parser<T>[] parsers) => new OneOfParser<T>(parsers.ToImmutableArray());

        public static Parser<T> ThenFixed<T>(this Parser<T> parser, string @fixed)
            => new ThenFixedParser<T>(parser, @fixed);
        private sealed class ThenFixedParser<T> : Parser<T>
        {
            private readonly Parser<T> _parser;
            private readonly string _fixed;

            public ThenFixedParser(Parser<T> parser, string @fixed)
            {
                _parser = parser;
                _fixed = @fixed;
            }

            public override Result<PartialParsed<T>> ParsePartial(Span input)
            {
                return _parser.ParsePartial(input).TryGetValue(out var parsed) &&
                       parsed.Remaining.Input[parsed.Remaining.Cursor..(parsed.Remaining.Cursor+parsed.Remaining.Length)].StartsWith(_fixed)
                    ? Result.Okay(parsed.Advance(_fixed.Length))
                    : default;
            }
        }

        // Linq-Monad-Mapping
        public static Parser<TResult> Select<TSource, TResult>(this Parser<TSource> parser, Func<TSource, TResult> map)
            => new SelectParser<TSource, TResult>(parser, map);
        private sealed class SelectParser<TSource, TResult> : Parser<TResult>
        {
            private readonly Parser<TSource> _parser;
            private readonly Func<TSource, TResult> _map;
            public SelectParser(Parser<TSource> parser, Func<TSource, TResult> map)
            {
                _parser = parser;
                _map = map;
            }

            public override Result<PartialParsed<TResult>> ParsePartial(Span input) =>
                _parser.ParsePartial(input).Map(t => t.Map(_map));
        }
        public static Parser<TResult> SelectMany<TSource, TCollection, TResult>(this Parser<TSource> source, Func<TSource, Parser<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            => new SelectManyParser<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
        private sealed class SelectManyParser<TSource, TCollection, TResult> : Parser<TResult>
        {
            private readonly Parser<TSource> _source;
            private readonly Func<TSource, Parser<TCollection>> _collectionSelector;
            private readonly Func<TSource, TCollection, TResult> _resultSelector;

            public SelectManyParser(Parser<TSource> source, Func<TSource, Parser<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            {
                _source = source ?? throw new ArgumentNullException(nameof(source));
                _collectionSelector = collectionSelector ?? throw new ArgumentNullException(nameof(collectionSelector));
                _resultSelector = resultSelector ?? throw new ArgumentNullException(nameof(resultSelector));
            }

            public override Result<PartialParsed<TResult>> ParsePartial(Span input)
            {
                var result = _source.ParsePartial(input);
                if (!result.TryGetValue(out var parsed))
                    return default;
                var nextParser = _collectionSelector(parsed.Value);
                var subResult = nextParser.ParsePartial(parsed.Remaining);
                if(!subResult.TryGetValue(out var nextParsed))
                    return default;
                var realResult = _resultSelector(parsed.Value, nextParsed.Value);
                return Result.Okay(PartialParsed.Create(realResult, nextParsed.Remaining));
            }
        }
    }
}
