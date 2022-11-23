using ProblemsLibrary;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AventOfCode._2015
{
	[Problem("2015-12-1")]
	public class Day12
	{
		[JsonConverter(typeof(CustomSerializer))]
		struct SumAllNumbers
		{
			public readonly int Value;

			public SumAllNumbers(int value)
			{
				this.Value = value;
			}
			class CustomSerializer : JsonConverter<SumAllNumbers>
			{
				public override SumAllNumbers Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
				{
					int sum = 0;
					do
					{
						switch (reader.TokenType)
						{
							case JsonTokenType.Number when reader.TryGetInt32(out var value):
								sum += value;
								break;
						}
					} while (reader.Read());
					return new SumAllNumbers(sum);
				}

				public override void Write(Utf8JsonWriter writer, SumAllNumbers value, JsonSerializerOptions options) { throw new NotImplementedException(); }
			}
		}

		[TestCase("[1,2,3]", 6)]
		[TestCase("{\"a\":2,\"b\":4}", 6)]
		[TestCase("{\"a\":{\"b\":4},\"c\":-1}", 3)]
		[TestCase("{\"a\":[-1,1]}", 0)]
		[TestCase("[-1,{\"a\":1}]", 0)]
		[TestCase("[]", 0)]
		[TestCase("{}", 0)]
		[TestCase("10", 10)]
		[TestCase("-2147483648", -2147483648)]
		public int Execute(string input) => JsonSerializer.Deserialize<SumAllNumbers>(input).Value;
	}

	[Problem("2015-12-2")]
	public class Day12Part2
	{
		[JsonConverter(typeof(CustomSerializer))]
		struct SumAllNumbers
		{
			public readonly int Value;

			public SumAllNumbers(int value)
			{
				Value = value;
			}
			class CustomSerializer : JsonConverter<SumAllNumbers>
			{
				public override SumAllNumbers Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
				{
					var stack = new Stack<int?>();
					stack.Push(0);
					bool hasMore = true;
					while(hasMore)
					{
						switch (reader.TokenType)
						{
							case JsonTokenType.Number when reader.TryGetInt32(out var value):
								stack.Push(stack.Pop() + value);
								hasMore = reader.Read();
								break;
							case JsonTokenType.StartObject:
								stack.Push(0);
								hasMore = reader.Read();
								break;
							case JsonTokenType.PropertyName:
								{
									hasMore = reader.Read();
									if (reader.TokenType == JsonTokenType.String && reader.GetString() == "red")
									{
										stack.Pop();
										stack.Push(null);
										hasMore = reader.Read();
									}
								}
								break;
							case JsonTokenType.EndObject:
								stack.Push((stack.Pop() ?? 0)+ stack.Pop());
								hasMore = reader.Read();
								break;
							default:
								hasMore = reader.Read();
								break;
						}
					}
					return new SumAllNumbers(stack.Pop() ?? 0);
				}

				public override void Write(Utf8JsonWriter writer, SumAllNumbers value, JsonSerializerOptions options) { throw new NotImplementedException(); }
			}
		}

		[TestCase("[1,2,3]", 6)]
		[TestCase(@"[1,{""c"":""red"",""b"":2},3]", 4)]
		[TestCase(@"{""d"":""red"",""e"":[1,2,3,4],""f"":5}", 0)]
		[TestCase("[1,\"red\",5]", 6)]
		[TestCase("{}", 0)]
		[TestCase("[]", 0)]
		public int Execute(string input) => JsonSerializer.Deserialize<SumAllNumbers>(input).Value;
	}
}
