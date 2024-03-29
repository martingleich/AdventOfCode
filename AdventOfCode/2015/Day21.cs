﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;
using static AdventOfCode.Utils.Utilities;

namespace AdventOfCode._2015;

public class Day21_Common
{
    private const string ItemsText = @"
Weapons:    Cost  Damage  Armor
Dagger        8     4       0
Shortsword   10     5       0
Warhammer    25     6       0
Longsword    40     7       0
Greataxe     74     8       0

Armor:      Cost  Damage  Armor
Leather      13     0       1
Chainmail    31     0       2
Splintmail   53     0       3
Bandedmail   75     0       4
Platemail   102     0       5

Rings:      Cost  Damage  Armor
Damage +1    25     1       0
Damage +2    50     2       0
Damage +3   100     3       0
Defense +1   20     0       1
Defense +2   40     0       2
Defense +3   80     0       3";

    private static readonly ImmutableArray<Item> Items = Parse(ItemsText).ToImmutableArray();
    private static readonly ImmutableArray<Item> Weapons = Items.Where(i => i.Category == "Weapons").ToImmutableArray();
    private static readonly ImmutableArray<Item> Armors = Items.Where(i => i.Category == "Armor").ToImmutableArray();
    private static readonly ImmutableArray<Item> Rings = Items.Where(i => i.Category == "Rings").ToImmutableArray();

    private static IEnumerable<Item> Parse(string input)
    {
        var categoryParser = Parser.MakeRegexParser(@"^(\w+):\s+Cost\s+Damage\s+Armor$", m => m.Groups[1].Value);
        var itemParser = Parser.MakeRegexParser(@"^(\w+|\w+ \+\d)\s+(\d+)\s+(\d+)\s+(\d+)$", m =>
        {
            var name = m.Groups[1].Value;
            var cost = int.Parse(m.Groups[2].ValueSpan);
            var damage = int.Parse(m.Groups[3].ValueSpan);
            var armor = int.Parse(m.Groups[4].ValueSpan);
            return (Func<string, Item>)(category => new Item(category, name, cost, damage, armor));
        });
        string? currentCategory = null;
        foreach (var line in input.SplitLines(true))
            if (categoryParser.TryParse(line).TryGetValue(out var category))
                currentCategory = category;
            else if (itemParser.TryParse(line).TryGetValue(out var item))
                yield return item(currentCategory!);
        // Weapons are required.
        yield return Item.Nothing("Armor");
        yield return Item.Nothing("Rings");
    }

    private static Character ParseCharacter(string input)
    {
        return new Character(100, new Equipment(8, 2, "boss"));
    }

    private static bool DoesFirstWin(Character firstPlayer, Character secondPlayer)
    {
        var damageByFirst = Math.Max(1, firstPlayer.Equipment.Damage - secondPlayer.Equipment.Armor);
        var damageBySecond = Math.Max(1, secondPlayer.Equipment.Damage - firstPlayer.Equipment.Armor);
        return damageByFirst >= damageBySecond;
    }

    public static int Execute(string input, bool part1)
    {
        var boss = ParseCharacter(input);
        var bothHandRings = (from right in Rings
                from left in Rings
                where left != right || (right.IsNothing() && left.IsNothing())
                let equipment = Equipment.Combine(Equipment.FromItem(right), Equipment.FromItem(left))
                select KeyValuePair.Create(equipment, left.Cost + right.Cost))
            .Distinct();
        var allItemGroups = ImmutableArray.Create(
            Weapons.Select(i => i.ToEquimentWithCost()),
            Armors.Select(i => i.ToEquimentWithCost()),
            bothHandRings);
        var cheapestEquiment = allItemGroups
            .AllCombinatorialSums(Equipment.Combine, part1 ? Comparer<int>.Default : ReverseComparer<int>.Default)
            .First(e => part1 == DoesFirstWin(new Character(100, e.Key), boss));
        return cheapestEquiment.Value;
    }

    private record Item(string Category, string Name, int Cost, int Damage, int Armor)
    {
        public static Item Nothing(string category)
        {
            return new(category, "", 0, 0, 0);
        }

        public bool IsNothing()
        {
            return Name == "";
        }

        public KeyValuePair<Equipment, int> ToEquimentWithCost()
        {
            return new(Equipment.FromItem(this), Cost);
        }
    }

    private record Equipment(int Damage, int Armor, string Name)
    {
        public static Equipment FromItem(Item item)
        {
            return new(item.Damage, item.Armor, item.Name);
        }

        public static Equipment Combine(Equipment a, Equipment b)
        {
            return new(a.Damage + b.Damage, a.Armor + b.Armor,
                a.Name.CompareTo(b.Name) <= 0 ? a.Name + b.Name : b.Name + a.Name);
        }
    }

    private record Character(int HitPoints, Equipment Equipment)
    {
    }
}

[Problem("2015-21-01")]
public class Day21_Part1
{
    public int Execute(string input)
    {
        return Day21_Common.Execute(input, true);
    }
}

[Problem("2015-21-02")]
public class Day21_Part2
{
    public int Execute(string input)
    {
        return Day21_Common.Execute(input, false);
    }
}