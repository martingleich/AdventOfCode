using ProblemsLibrary;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AventOfCode._2015
{
	[Problem("2015-15")]
	public class Day15
	{
		public class Ingredient
		{
			public readonly string Name;
			public readonly int Calories;
			public readonly ImmutableDictionary<string, int> Props;

			public Ingredient(string name, ImmutableDictionary<string, int> props, int calories)
			{
				Name = name;
				Props = props;
				Calories = calories;
			}

			public static Ingredient FromText(string input)
			{
				var x = input.Split(":");
				var y = x[1].Split(",");
				var props = ImmutableDictionary.CreateBuilder<string, int>();
				int calories = 0;
				for (int i = 0; i < y.Length; ++i)
				{
					var v = y[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);
					int value = int.Parse(v[1]);
					if (v[0] == "calories")
						calories = value;
					else
						props.Add(v[0], value);
				}
				return new Ingredient(x[0], props.ToImmutable(), calories);
			}
		}

		[TestCase(@"Butterscotch: capacity -1, durability -2, flavor 6, texture 3, calories 8
Cinnamon: capacity 2, durability 3, flavor -2, texture -1, calories 3", 62842880)]
		public int Execute(string input)
		{
			return RealExecute(input, null);
		}
		public static int RealExecute(string input, int? maybeCalories)
		{
			var ingredients = input.SplitLines().Select(Ingredient.FromText).ToImmutableArray();
			var allRecipes = Utilities.SplitTotal(100, ingredients.Length).Select(c => Utilities.ZipTuple(ingredients, c));
			if (maybeCalories is int calories)
				allRecipes = allRecipes.Where(r => calories == r.Sum(i => i.Item1.Calories * i.Item2));
			return allRecipes.Max(GetQuality);
		}

		public static int GetQuality(IEnumerable<(Ingredient, int)> recipe)
		{
			Dictionary<string, int> props = new Dictionary<string, int>();
			foreach(var (i,c) in recipe)
			{
				foreach(var prop in i.Props)
				{
					props[prop.Key] = (props.TryGetValue(prop.Key, out int v) ? v : 0) + prop.Value * c;
				}
			}
			return props.Values.Aggregate(1, (a, b) => a * Math.Max(0, b));
		}
	}
	[Problem("2015-15-2")]
	public class Day15Part2
	{
		public int Execute(string input)
		{
			return Day15.RealExecute(input, 500);
		}
	}
}
