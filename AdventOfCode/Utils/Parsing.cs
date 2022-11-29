using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace AdventOfCode.Utils
{
    public abstract class Parser<T>
    {
        protected Parser() { }

        public Result<T> TryParse(string input) => ParsePartial(Span.FromString(input)).Value;
        public T Parse(string input) => TryParse(input).Value;
        public abstract ParseResult<T> ParsePartial(Span input);
    }
    public struct Span
    {
        public readonly string Input;
        public readonly int Cursor;

        private Span(string input, int cursor)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Cursor = cursor;
        }

        public static Span FromString(string input) => new(input, 0);
        public Span Advance(int count) => new(Input, Cursor + count);
    }
    public struct ParseResult<T>
    {
        public Result<T> Value;
        public Span Span;

        public static ParseResult<T> Okay(T realResult, Span subSpan) => new (Result.Okay(realResult), subSpan);
        public static ParseResult<T> Create(Result<T> value, Span span) => new(value, span);
        public static readonly ParseResult<T> Error;

        public ParseResult(Result<T> value, Span span)
        {
            Value = value;
            Span = span;
        }

        public bool HasValue => Value.HasValue;
        public bool TryGetValue([MaybeNull, NotNullWhen(true)] out T value, out Span span)
        {
            if (Value.TryGetValue(out value))
            {
                span = Span;
                return true;
            }
            else
            {
                span = default;
                return false;
            }
        }

    }
    public static class ParseResult
    {
        public static ParseResult<T> Okay<T>(T realResult, Span subSpan) => new (Result.Okay(realResult), subSpan);
        public static ParseResult<T> Create<T>(Result<T> value, Span span) => new(value, span);
    }
    public static class Parser
    {
        public readonly static Parser<string> AlphaNumeric = MakeRegexParser(new Regex(@"[a-zA-Z0-9]+"), m => m.Value);
        public readonly static Parser<int> Int32 = MakeRegexParser(new Regex(@"\d+"), m => int.Parse(m.Value));

        private sealed class RegexParser<T> : Parser<T>
        {
            private readonly Regex _regex;
            private readonly Func<Match, Result<T>> _converter;

            public RegexParser(Regex regex, Func<Match, Result<T>> converter)
            {
                _regex = regex ?? throw new ArgumentNullException(nameof(regex));
                _converter = converter ?? throw new ArgumentNullException(nameof(converter));
            }
            public override ParseResult<T> ParsePartial(Span input)
            {
                var m = _regex.Match(input.Input, input.Cursor);
                return m.Success ? ParseResult.Create(_converter(m), input.Advance(m.Length)) : ParseResult<T>.Error;
            }
        }

        public static Parser<T> MakeRegexParser<T>(string regex, Func<Match, T> converter) => MakeRegexParser(new Regex(regex), converter);
        public static Parser<T> MakeRegexParser<T>(Regex regex, Func<Match, T> converter) => new RegexParser<T>(regex, m => Result.Okay(converter(m)));

        private sealed class OneOfParser<T> : Parser<T>
        {
            private readonly ImmutableArray<Parser<T>> _alternatives;

            public OneOfParser(ImmutableArray<Parser<T>> alternatives)
            {
                _alternatives = alternatives;
            }

            public override ParseResult<T> ParsePartial(Span input)
            {
                foreach (var parser in _alternatives)
                {
                    var result = parser.ParsePartial(input);
                    if (result.HasValue)
                        return result;
                }
                return ParseResult<T>.Error;
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

            public override ParseResult<T> ParsePartial(Span input)
            {
                if (_parser.ParsePartial(input).TryGetValue(out var value, out var span))
                {
                    if (span.Input[span.Cursor..].StartsWith(_fixed))
                        return ParseResult.Okay(value, span.Advance(_fixed.Length));
                }
                return ParseResult<T>.Error;
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

            public override ParseResult<TResult> ParsePartial(Span input)
            {
                if (_parser.ParsePartial(input).TryGetValue(out var value, out var span))
                    return ParseResult.Okay(_map(value), span);
                return ParseResult<TResult>.Error;
            }
        }
        public static Parser<TResult> SelectMany<TSource, TCollection, TResult>(this Parser<TSource> source, Func<TSource, Parser<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            => new SelectManyParser<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
        private sealed class SelectManyParser<TSource, TCollection, TResult> : Parser<TResult>
        {
            private readonly Parser<TSource> Source;
            private readonly Func<TSource, Parser<TCollection>> CollectionSelector;
            private readonly Func<TSource, TCollection, TResult> ResultSelector;

            public SelectManyParser(Parser<TSource> source, Func<TSource, Parser<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            {
                Source = source ?? throw new ArgumentNullException(nameof(source));
                CollectionSelector = collectionSelector ?? throw new ArgumentNullException(nameof(collectionSelector));
                ResultSelector = resultSelector ?? throw new ArgumentNullException(nameof(resultSelector));
            }

            public override ParseResult<TResult> ParsePartial(Span input)
            {
                var result = Source.ParsePartial(input);
                if (!result.TryGetValue(out var value, out var span))
                    return ParseResult<TResult>.Error;
                var nextParser = CollectionSelector(value);
                var subResult = nextParser.ParsePartial(span);
                if(!subResult.TryGetValue(out var subValue, out var subSpan))
                    return ParseResult<TResult>.Error;
                var realResult = ResultSelector(value, subValue);
                return ParseResult.Okay(realResult, subSpan);
            }
        }
    }
}
