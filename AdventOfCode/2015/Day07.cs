using AdventOfCode.Utils;
using ProblemsLibrary;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AventOfCode._2015
{
	[Problem("2015-07-1")]
	public class Day07
	{
		class Node
		{
			public readonly string Name;
			public readonly string Action;
			public readonly string[] Dependecies;

			public Node(string name, string action, params string[] dependecies)
			{
				Name = name;
				Action = action;
				Dependecies = dependecies;
			}

			public static Node FromString(string input)
			{
				var words = input.Split(" ");
				if (words.Length == 3) // x -> y
					return new Node(words[2], "", words[0]);
				else if (words.Length == 4) // NOT x -> y
					return new Node(words[3], words[0], words[1]);
				else // x Op y -> z
					return new Node(words[4], words[1], words[0], words[2]);
			}
		}

		public class Circuit
		{
			readonly ImmutableDictionary<string, Node> Graph;

			private Circuit(ImmutableDictionary<string, Node> graph)
			{
				Graph = graph;
			}

			public ushort Query(string wire, params KeyValuePair<string, ushort>[] initialValues)
			{
				var requestStack = new Stack<object>();
				var resultStack = new Stack<ushort>();
				var values = initialValues.ToDictionary(v => v.Key, v => v.Value);
				requestStack.Push(wire);
				while (requestStack.TryPop(out var request))
				{
					if (request is Tuple<string, string> action)
					{
						var result = action.Item1 switch
						{
							"AND" => (ushort)(resultStack.Pop() & resultStack.Pop()),
							"OR" => (ushort)(resultStack.Pop() | resultStack.Pop()),
							"LSHIFT" => (ushort)(resultStack.Pop() << resultStack.Pop()),
							"RSHIFT" => (ushort)(resultStack.Pop() >> resultStack.Pop()),
							"NOT" => unchecked((ushort)~resultStack.Pop()),
							_ => resultStack.Pop(),
						};
						values[action.Item2] = result;
						resultStack.Push(result);
					}
					else
					{
						var nextName = (string)request;
						if (char.IsDigit(nextName[0]))
						{
							resultStack.Push(ushort.Parse(nextName));
						}
						else if (values.TryGetValue(nextName, out var value))
						{
							resultStack.Push(value);
						}
						else
						{
							var node = Graph[nextName];
							requestStack.Push(Tuple.Create(node.Action, node.Name));
							foreach (var dep in node.Dependecies)
								requestStack.Push(dep);
						}
					}
				}

				return resultStack.Pop();
			}


			public static Circuit FromString(string input)
			{
				var graph = input.SplitLines().Select(Node.FromString).ToImmutableDictionary(n => n.Name, n => n);
				return new Circuit(graph);
			}
		}

		[TestCase("123 -> a", 123)]
		[TestCase("1 AND 1 -> a", 1)]
		[TestCase("1 AND 0 -> a", 0)]
		[TestCase("1 OR 0 -> a", 1)]
		[TestCase("1 LSHIFT 3 -> a", 8)]
		[TestCase("8 RSHIFT 3 -> a", 1)]
		[TestCase("NOT 65535 -> a", 0)]
		[TestCase(@"123 -> x
456 -> y
i -> a
x AND y -> i", 72)]
		public int Execute(string input)
		{
			var circuit = Circuit.FromString(input);
			return circuit.Query("a");
		}
	}

	[Problem("2015-07-2")]
	public class Day07Part2
	{
		public int Execute(string input)
		{
			var circuit = Day07.Circuit.FromString(input);
			var firstAValue = circuit.Query("a");
			return circuit.Query("a", KeyValuePair.Create("b", firstAValue));
		}
	}
}
