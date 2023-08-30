using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Utils;

public interface IParser
{
    Result<PartialParsed<object?>> ParseObject(Span input);
}

public abstract class Parser<T> : IParser
{
    Result<PartialParsed<object?>> IParser.ParseObject(Span input)
    {
        return ParsePartial(input).CastToObject();
    }

    public T Parse(string input)
    {
        return Parse(Span.FromString(input));
    }

    public T Parse(Span span)
    {
        return TryParse(span).Value;
    }

    public Result<T> TryParse(string input)
    {
        return TryParse(Span.FromString(input));
    }

    public Result<T> TryParse(Span span)
    {
        return ParsePartial(span).MapExtractPartialParsed();
    }

    public IEnumerable<T> ParseRepeated(string input)
    {
        var span = Span.FromString(input);
        while (ParsePartial(span).TryGetValue(out var parsed))
        {
            yield return parsed.Value;
            span = parsed.Remaining;
        }
    }

    public IEnumerable<T> ParseValidLines(string input)
    {
        return input.SplitLines().Select(TryParse).WhereOkay();
    }

    public abstract Result<PartialParsed<T>> ParsePartial(Span input);
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

    public PartialParsed<TResult> Map<TResult>(Func<T, TResult> map)
    {
        return PartialParsed.Create(map(Value), Remaining);
    }

    public PartialParsed<object?> CastToObject()
    {
        return PartialParsed.Create((object?)Value, Remaining);
    }

    public PartialParsed<T> Advance(int count)
    {
        return PartialParsed.Create(Value, Remaining.Advance(count));
    }
}

public static class PartialParsed
{
    public static PartialParsed<T> Create<T>(T value, Span remaining)
    {
        return new PartialParsed<T>(value, remaining);
    }
}

public static class Parser
{
    public static readonly Parser<string> AlphaNumeric =
        MakeRegexParser(new Regex(@"^[a-zA-Z0-9]+", RegexOptions.Compiled), m => m.Value);

    public static readonly Parser<int> UnsignedInteger = new IntegerParser(true);
    public static readonly Parser<int> SignedInteger = new IntegerParser(false);
    public static readonly Parser<int> SingleDigit = new SingleDigitParser();

    public static readonly Parser<char> Char =
        MakeRegexParser(new Regex(@"^[a-zA-Z]", RegexOptions.Compiled), m => m.Value[0]);

    public static readonly Parser<string> NewLine =
        MakeRegexParser(new Regex(@"^\n|(\r\n)", RegexOptions.Compiled), m => m.Value);

    public static readonly Parser<string> Whitespace =
        MakeRegexParser(new Regex(@"^\s+", RegexOptions.Compiled), m => m.Value);

    public static readonly Parser<string> EmptyLine =
        MakeRegexParser(new Regex(@"^(\n|\r\n)( *)(\n|\r\n)", RegexOptions.Compiled), m => m.Value);

    public static readonly Parser<string> AnyWhitespace =
        MakeRegexParser(new Regex(@"^\s*", RegexOptions.Compiled), m => m.Value);

    public static readonly Parser<string> Line =
        MakeRegexParser(new Regex(@"^([^\r\n]*)(\n|\r\n|$)", RegexOptions.Compiled), m => m.Groups[1].Value);

    public static readonly Parser<int> EnglishNumber = Parser.OneOf(
        Parser.Fixed("first").Return(1),
        Parser.Fixed("second").Return(2),
        Parser.Fixed("third").Return(3),
        Parser.Fixed("fourth").Return(4));
    private static int? TryReadDigit(char c)
    {
        if (c >= '0' && c <= '9')
            return c - '0';
        return null;
    }

    public static Parser<T> Recursive<T>(Func<Parser<T>, Parser<T>> func)
    {
        var reference = new ParserRef<T>();
        return reference.Parser = func(reference);
    }

    private sealed class ParserRef<T> : Parser<T>
    {
        public Parser<T>? Parser { set; get; }

        public override Result<PartialParsed<T>> ParsePartial(Span input)
        {
            if (Parser == null)
                throw new InvalidOperationException("Parser not set yet.");
            return Parser.ParsePartial(input);
        }
    }

    public static Parser<TResult> Return<TInput, TResult>(this Parser<TInput> parser, TResult result)
    {
        return new ReturnParser<TResult, TInput>(parser, result);
    }

    public static Parser<string> Fixed(string value)
    {
        return new FixedParser(value);
    }

    public static Parser<IEnumerable<T>> Repeat<T>(this Parser<T> parser)
    {
        return new RepeatParser<T>(parser);
    }

    public static Parser<T2> IgnoreThen<T1, T2>(this Parser<T1> first, Parser<T2> second)
    {
        return from f in first
            from s in second
            select s;
    }

    public static Parser<T1> ThenIgnore<T1, T2>(this Parser<T1> first, Parser<T2> second)
    {
        return from f in first
            from s in second
            select f;
    }

    public static Parser<Match> ToParser(this Regex regex)
    {
        return new RegexParser(regex);
    }

    public static Parser<T> MakeRegexParser<T>(string regex, Func<Match, T> converter)
    {
        return MakeRegexParser(new Regex(regex), converter);
    }

    public static Parser<T> MakeRegexParser<T>(Regex regex, Func<Match, T> converter)
    {
        return regex.ToParser().Select(converter);
    }

    public static Parser<Empty> FormattedString(FormattableString str)
    {
        return FormattedString(str, x => default(Empty));
    }

    public static Parser<T> FormattedString<T>(FormattableString str, Func<ImmutableArray<dynamic>, T> converter)
    {
        return FormattedStringBasic(str).Select(converter);
    }

    private static Parser<ImmutableArray<object>> FormattedStringBasic(FormattableString str)
    {
        var fixedStrings = ImmutableArray.CreateBuilder<string>();
        var parsers = ImmutableArray.CreateBuilder<IParser>();
        var format = str.Format;
        var args = str.GetArguments();
        var fix = "";
        for (var i = 0; i < format.Length; ++i)
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
            }
            else
            {
                fix += format[i];
            }

        fixedStrings.Add(fix);
        return new FormatStringParser(fixedStrings.ToImmutable(), parsers.ToImmutable());
    }

    public static Parser<IEnumerable<TValue>> DelimitedWith<TValue, TSeperator>(this Parser<TValue> value,
        Parser<TSeperator> seperator)
    {
        return new SeperatedParser<TSeperator, TValue>(seperator, value);
    }

    public static Parser<T> Bracket<T>(this Parser<T> parser, string leading, string trailing)
    {
        return from l in Fixed(leading)
            from v in parser
            from t in Fixed(trailing)
            select v;
    }

    public static Parser<T> Trimmed<T>(this Parser<T> parser)
    {
        return new TrimmedParser<T>(parser);
    }

    public static Parser<T> OneOf<T>(params Parser<T>[] parsers)
    {
        return new OneOfParser<T>(parsers.ToImmutableArray());
    }

    public static Parser<T> ThenFixed<T>(this Parser<T> parser, string @fixed)
    {
        return new ThenFixedParser<T>(parser, @fixed);
    }

    // Linq-Monad-Mapping
    public static Parser<TResult> Select<TSource, TResult>(this Parser<TSource> parser, Func<TSource, TResult> map)
    {
        return new SelectParser<TSource, TResult>(parser, map);
    }

    public static Parser<TResult> SelectMany<TSource, TCollection, TResult>(this Parser<TSource> source,
        Func<TSource, Parser<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
    {
        return new SelectManyParser<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
    }

    public static Parser<T> Where<T>(this Parser<T> source, Func<T, bool> filter)
    {
        return new WhereParser<T>(source, filter);
    }

    private sealed class IntegerParser : Parser<int>
    {
        private readonly bool _unsigned;

        public IntegerParser(bool unsigned)
        {
            _unsigned = unsigned;
        }

        public override Result<PartialParsed<int>> ParsePartial(Span input)
        {
            bool sign;
            if (!_unsigned && input.Length > 0 && (input[0] == '-' || input[0] == '+'))
            {
                sign = input[0] == '-';
                input = input.Advance(1);
            }
            else
            {
                sign = false;
            }

            var anyDigits = false;
            var r = 0L;
            while (input.Length > 0 && TryReadDigit(input.Input[input.Cursor]) is { } d)
            {
                r = r * 10 + d;
                input = input.Advance(1);
                anyDigits = true;
            }

            var realValue = (int)(sign ? -r : r);
            return anyDigits ? Result.Okay(PartialParsed.Create(realValue, input)) : default;
        }
        public override string? ToString() => "Int";
    }

    private sealed class SingleDigitParser : Parser<int>
    {
        public override Result<PartialParsed<int>> ParsePartial(Span input)
        {
            if (input.Length > 0 && TryReadDigit(input[0]) is { } d)
                return Result.Okay(PartialParsed.Create(d, input.Advance(1)));
            return default;
        }
        public override string? ToString() => "SingleDigit";
    }

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
            return Result.Okay(PartialParsed.Create(_value, parsed.Remaining));
        }
        public override string? ToString() => _parser.ToString();
    }

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
        public override string ToString() => _value;
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
        public override string ToString() => _regex.ToString();
    }

    private sealed class RepeatParser<T> : Parser<IEnumerable<T>>
    {
        private readonly Parser<T> _parser;

        public RepeatParser(Parser<T> parser)
        {
            _parser = parser;
        }

        public override Result<PartialParsed<IEnumerable<T>>> ParsePartial(Span input)
        {
            var results = default(List<T>?);
            while (_parser.ParsePartial(input).TryGetValue(out var parsed))
            {
                results ??= new List<T>();
                results.Add(parsed.Value);
                input = parsed.Remaining;
            }

            return Result.Okay(PartialParsed.Create(results?.AsReadOnly().AsEnumerable() ?? Enumerable.Empty<T>(),
                input));
        }
        public override string ToString() => $"Repeat({_parser})";
    }

    private sealed class FormatStringParser : Parser<ImmutableArray<object>>
    {
        private readonly ImmutableArray<string> _fixed;
        private readonly ImmutableArray<IParser> _parsers;

        public FormatStringParser(ImmutableArray<string> @fixed, ImmutableArray<IParser> parsers)
        {
            _fixed = @fixed;
            _parsers = parsers;
        }

        public override Result<PartialParsed<ImmutableArray<object>>> ParsePartial(Span input)
        {
            var builder = ImmutableArray.CreateBuilder<object>(_parsers.Length);
            for (var i = 0; i < _fixed.Length + _parsers.Length; ++i)
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

            return Result.Okay(PartialParsed.Create(builder.ToImmutable(), input));
        }
    }

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
        public override string ToString() => $"({string.Join("|", _alternatives)})";
    }

    private sealed class ThenFixedParser<T> : Parser<T>
    {
        private readonly string _fixed;
        private readonly Parser<T> _parser;

        public ThenFixedParser(Parser<T> parser, string @fixed)
        {
            _parser = parser;
            _fixed = @fixed;
        }

        public override Result<PartialParsed<T>> ParsePartial(Span input)
        {
            return _parser.ParsePartial(input).TryGetValue(out var parsed) &&
                   parsed.Remaining.Input[parsed.Remaining.Cursor..(parsed.Remaining.Cursor + parsed.Remaining.Length)]
                       .StartsWith(_fixed)
                ? Result.Okay(parsed.Advance(_fixed.Length))
                : default;
        }
    }

    private sealed class SelectParser<TSource, TResult> : Parser<TResult>
    {
        private readonly Func<TSource, TResult> _map;
        private readonly Parser<TSource> _parser;

        public SelectParser(Parser<TSource> parser, Func<TSource, TResult> map)
        {
            _parser = parser;
            _map = map;
        }

        public override Result<PartialParsed<TResult>> ParsePartial(Span input)
        {
            return _parser.ParsePartial(input).MapPartialParsed(_map);
        }
    }

    private sealed class SelectManyParser<TSource, TCollection, TResult> : Parser<TResult>
    {
        private readonly Func<TSource, Parser<TCollection>> _collectionSelector;
        private readonly Func<TSource, TCollection, TResult> _resultSelector;
        private readonly Parser<TSource> _source;

        public SelectManyParser(Parser<TSource> source, Func<TSource, Parser<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
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
            if (!subResult.TryGetValue(out var nextParsed))
                return default;
            var realResult = _resultSelector(parsed.Value, nextParsed.Value);
            return Result.Okay(PartialParsed.Create(realResult, nextParsed.Remaining));
        }
    }

    private sealed class WhereParser<T> : Parser<T>
    {
        private readonly Func<T, bool> _filter;
        private readonly Parser<T> _source;

        public WhereParser(Parser<T> source, Func<T, bool> filter)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        public override Result<PartialParsed<T>> ParsePartial(Span input)
        {
            if (!_source.ParsePartial(input).TryGetValue(out var parsed))
                return default;
            if (!_filter(parsed.Value))
                return default;
            return Result.Okay(parsed);
        }
    }

    public static Parser<Matrix<T>> Grid<T, TSep>(this Parser<T> self, Parser<TSep> lineSep)
        => new GridParser<T, TSep>(self, lineSep);

    private sealed class GridParser<T, TSep> : Parser<Matrix<T>>
    {
        private readonly Parser<T> _parser;
        private readonly Parser<TSep> _parserSeperator;

        public GridParser(Parser<T> parser, Parser<TSep> parserSeperator)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _parserSeperator = parserSeperator ?? throw new ArgumentNullException(nameof(parserSeperator));
        }

        public override Result<PartialParsed<Matrix<T>>> ParsePartial(Span input)
        {
            var elements = new List<T>();
            var finalWidth = default(int?);
            var width = 0;
            var height = 0;
            while (true)
            {
                if (_parser.ParsePartial(input).TryGetValue(out var parsed))
                {
                    elements.Add(parsed.Value);
                    input = parsed.Remaining;
                    ++width;
                }
                else
                {
                    if (width == 0)
                        break; // Allow a trailing seperator
                    if (finalWidth is int fw && fw != width)
                        return default;
                    else
                        finalWidth = width;
                    width = 0;
                    ++height;
                    if (_parserSeperator.ParsePartial(input).TryGetValue(out var parsedSep))
                        input = parsedSep.Remaining;
                    else
                        break;
                }
            }

            var result = finalWidth is int fw2
                ? Matrix.FromEnumerable(fw2, height, Matrix.Ordering.RowMajor, elements)
                : Matrix<T>.Empty;
            return Result.Okay(PartialParsed.Create(result, input));
        }
    }
}