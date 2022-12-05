using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Utils
{
    public interface IParser
    {
        Result<PartialParsed<object?>> ParseObject(Span input);
    }
    public abstract class Parser<T> : IParser
    {
        public T Parse(string input) => Parse(Span.FromString(input));
        public T Parse(Span span) => TryParse(span).Value;
        public Result<T> TryParse(string input) => TryParse(Span.FromString(input));
        public Result<T> TryParse(Span span) => ParsePartial(span).Map(v => v.Value);
        public abstract Result<PartialParsed<T>> ParsePartial(Span input);
        Result<PartialParsed<object?>> IParser.ParseObject(Span input) => ParsePartial(input).Map(v => v.Map(v => (object?)v));
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
        public static readonly Parser<string> AlphaNumeric = MakeRegexParser(new Regex(@"^[a-zA-Z0-9]+"), m => m.Value);
        public static readonly Parser<int> Int32 = MakeRegexParser(new Regex(@"^\d+"), m => int.Parse(m.Value));
        public static readonly Parser<string> NewLine = MakeRegexParser(new Regex(@"^\n|(\r\n)"), m => m.Value);
        public static readonly Parser<string> Whitespace = MakeRegexParser(new Regex(@"^\s+"), m => m.Value);

        public static Parser<TResult> Return<TInput, TResult>(this Parser<TInput> parser, TResult result)
            => new ReturnParser<TResult, TInput>(parser, result);

        private sealed class ReturnParser<TResult, TInput> : Parser<TResult>
        {
            private readonly Parser<TInput> _parser;
            private readonly TResult _value;

            public ReturnParser(Parser<TInput> parser, TResult value)
            {
                _parser = parser;
                _value = value;
            }

            public override Result<PartialParsed<TResult>> ParsePartial(Span input)
            {
                if (!_parser.ParsePartial(input).TryGetValue(out var parsed))
                    return default;
                return Result.Okay(PartialParsed.Create<TResult>(_value, parsed.Remaining));
            }
        }

        public static Parser<string> Fixed(string value) => new FixedParser(value);

        private sealed class FixedParser : Parser<string>
        {
            private readonly string _value;

            public FixedParser(string value)
            {
                _value = value ?? throw new ArgumentNullException(nameof(value));
            }

            public override Result<PartialParsed<string>> ParsePartial(Span input)
            {
                if (input.StartsWith(_value))
                    return Result.Okay(PartialParsed.Create(_value, input.Advance(_value.Length)));
                return default;
            }
        }

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
        public static Parser<T> FormattedString<T>(FormattableString str, Func<ImmutableArray<dynamic>, T> converter)
        {
            var fixedStrings = ImmutableArray.CreateBuilder<string>();
            var parsers = ImmutableArray.CreateBuilder<IParser>();
            var format = str.Format;
            var args = str.GetArguments();
            string fix = "";
            for (int i = 0; i < format.Length; ++i)
            {
                if (i + 1 < format.Length && format[i] == '{' && format[i + 1] == '{')
                {
                    fix += '{';
                    ++i;
                }
                else if (i + 1 < format.Length && format[i] == '}' && format[i + 1] == '}')
                {
                    fix += '{';
                    ++i;
                }
                else if (format[i] == '{')
                {
                    // Read inner block
                    fixedStrings.Add(fix);
                    fix = "";
                    ++i;
                    var start = i;
                    while (i < format.Length && format[i] != '}')
                        ++i;
                    var index = int.Parse(format[start..i]);
                    if (args[index] is IParser parser)
                        parsers.Add(parser);
                    else
                        throw new InvalidOperationException();
                } else
                {
                    fix += format[i];
                }
            }
            fixedStrings.Add(fix);
            return new FormatStringParser(fixedStrings.ToImmutable(), parsers.ToImmutable()).Select(converter);
        }
        private sealed class FormatStringParser : Parser<ImmutableArray<dynamic>>
        {
            private readonly ImmutableArray<string> _fixed;
            private readonly ImmutableArray<IParser> _parsers;

            public FormatStringParser(ImmutableArray<string> @fixed, ImmutableArray<IParser> parsers)
            {
                _fixed = @fixed;
                _parsers = parsers;
            }

            public override Result<PartialParsed<ImmutableArray<dynamic>>> ParsePartial(Span input)
            {
                var builder = ImmutableArray.CreateBuilder<dynamic>(_parsers.Length);
                for(int i = 0; i < _fixed.Length + _parsers.Length; ++i)
                {
                    if (i % 2 == 0)
                    {
                        if (!input.StartsWith(_fixed[i / 2]))
                            return default;
                        input = input.Advance(_fixed[i / 2].Length);
                    }
                    else
                    {
                        var result = _parsers[i / 2].ParseObject(input);
                        if (!result.TryGetValue(out var value))
                            return default;
                        input = value.Remaining;
                        builder.Add(value.Value!); // We convert to dynamic anyways no point in caring about nullablitly
                    }
                }

                return Result.Okay(PartialParsed.Create(builder.ToImmutable(), input));
            }
        }

        public static Parser<IEnumerable<TValue>> DelimitedWith<TValue, TSeperator>(this Parser<TValue> value, Parser<TSeperator> seperator)
            => new SeperatedParser<TSeperator, TValue>(seperator, value);
        public static Parser<T> Trimmed<T>(this Parser<T> parser) => new TrimmedParser<T>(parser);
        private sealed class TrimmedParser<TValue> : Parser<TValue>
        {
            private readonly Parser<TValue> _value;

            public TrimmedParser(Parser<TValue> value)
            {
                _value = value ?? throw new ArgumentNullException(nameof(value));
            }

            public override Result<PartialParsed<TValue>> ParsePartial(Span input)
            {
                while (input.Length > 0 && char.IsWhiteSpace(input.Input[input.Cursor]))
                    input = input.Advance(1);
                if (!_value.ParsePartial(input).TryGetValue(out var value))
                    return default;
                input = value.Remaining;
                while (input.Length > 0 && char.IsWhiteSpace(input.Input[input.Cursor]))
                    input = input.Advance(1);
                return Result.Okay(PartialParsed.Create(value.Value, input));
            }
        }

        private sealed class SeperatedParser<TSeperator, TValue> : Parser<IEnumerable<TValue>>
        {
            private readonly Parser<TSeperator> _seperator;
            private readonly Parser<TValue> _value;

            public SeperatedParser(Parser<TSeperator> seperator, Parser<TValue> value)
            {
                _seperator = seperator ?? throw new ArgumentNullException(nameof(seperator));
                _value = value ?? throw new ArgumentNullException(nameof(value));
            }

            public override Result<PartialParsed<IEnumerable<TValue>>> ParsePartial(Span input)
            {
                var result = _value.ParsePartial(input);
                if (!result.TryGetValue(out var firstValue))
                    return Result.Okay(PartialParsed.Create(Enumerable.Empty<TValue>(), input));
                var values = new List<TValue>
                {
                    firstValue.Value
                };
                input = firstValue.Remaining;
                while (_seperator.ParsePartial(input).TryGetValue(out var sep))
                {
                    var result2 = _value.ParsePartial(sep.Remaining);
                    if (!result2.TryGetValue(out var innerValue))
                        return default; // Expected token after seperator.
                    values.Add(innerValue.Value);
                    input = innerValue.Remaining;
                }
                return Result.Okay(PartialParsed.Create(values.AsReadOnly().AsEnumerable(), input));
            }
        }
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
