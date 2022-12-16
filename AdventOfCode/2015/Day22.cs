using System;
using System.Collections.Immutable;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AdventOfCode._2015;

public class Day22_Common
{
    private static State Step(State state, Spell action, int bossDamage, bool hardMode)
    {
        // Player turn
        // Apply effects
        if (hardMode)
        {
            state = state with { PlayerHealth = state.PlayerHealth - 1 };
            if (state.PlayerHealth <= 0)
                return state;
        }

        state = state.ApplyEffects();

        // Cast a spell
        var spellCost = action.Cost;
        if (spellCost > state.PlayerMana)
            throw new InvalidOperationException();
        state = state with
        {
            PlayerMana = state.PlayerMana - spellCost, SpendMana = state.SpendMana + spellCost,
            History = state.History.Add(action)
        };
        state = action.Cast(state);
        if (state.BossHealth <= 0)
            return state;

        // Boss turn
        state = state.ApplyEffects();
        if (state.BossHealth <= 0)
            return state;
        // Attack the player
        var playerArmor = state.ShieldEffect > 0 ? 7 : 0;
        var damage = Math.Max(1, bossDamage - playerArmor);
        state = state with { PlayerHealth = state.PlayerHealth - damage };
        return state;
    }


    private static State? FindMinimum(State state, State? runningMin, int bossDamage, bool hardMode)
    {
        var afterEffectState = state.ApplyEffects();
        foreach (var action in Spell.AllActions)
        {
            if (action.Cost > afterEffectState.PlayerMana)
                break; // Since actions are ordered by cost we can abort as soon as one misses.
            if (!action.CanCastIgnoreCost(afterEffectState))
                continue; // If for some reason we cannot cast, just skip it.
            var newState = Step(state, action, bossDamage, hardMode);
            if (newState.BossHealth <= 0)
            {
                if (!runningMin.HasValue || newState.SpendMana < runningMin.Value.SpendMana)
                    runningMin = newState;
                continue;
            }

            if (newState.PlayerHealth <= 0)
                continue;
            if (!runningMin.HasValue || newState.SpendMana < runningMin.Value.SpendMana)
                if (FindMinimum(newState, runningMin, bossDamage, hardMode) is State minWinState)
                    if (!runningMin.HasValue || minWinState.SpendMana < runningMin.Value.SpendMana)
                        runningMin = minWinState;
        }

        return runningMin;
    }

    private static BossData ParseBossData(string input)
    {
        var lines = input.SplitLines(true).ToArray();
        var hitPoints = Parser.MakeRegexParser(@"Hit Points: (\d+)", g => int.Parse(g.Groups[1].ValueSpan))
            .Parse(lines[0]);
        var damage = Parser.MakeRegexParser(@"Damage: (\d+)", g => int.Parse(g.Groups[1].ValueSpan)).Parse(lines[1]);
        return new BossData(hitPoints, damage);
    }

    public static int Execute(string input, bool hardMode)
    {
        var bossData = ParseBossData(input);
        var minMana = FindMinimum(new State(50, 500, bossData.HitPoints, 0, 0, 0, 0, ImmutableList<Spell>.Empty), null,
            bossData.Damage, hardMode);
        return minMana!.Value.SpendMana;
    }

    private record struct State(
        int PlayerHealth,
        int PlayerMana,
        int BossHealth,
        int ShieldEffect,
        int PoisonEffect,
        int RechargeEffect,
        int SpendMana,
        ImmutableList<Spell> History)
    {
        public State ApplyEffects()
        {
            var state = this;
            if (state.RechargeEffect > 0)
                state = state with { PlayerMana = state.PlayerMana + 101, RechargeEffect = state.RechargeEffect - 1 };
            if (state.PoisonEffect > 0)
                state = state with { BossHealth = state.BossHealth - 3, PoisonEffect = state.PoisonEffect - 1 };
            if (state.ShieldEffect > 0)
                state = state with { ShieldEffect = state.ShieldEffect - 1 };
            return state;
        }
    }

    private record class Spell(int Cost, Func<State, State> Cast, Func<State, bool> CanCastIgnoreCost)
    {
        public static readonly Spell MagicMissile =
            new(53, state => state with { BossHealth = state.BossHealth - 4 }, state => true);

        public static readonly Spell Drain = new(73,
            state => state with { BossHealth = state.BossHealth - 2, PlayerHealth = state.PlayerHealth + 2 },
            state => true);

        public static readonly Spell Shield = new(113, state => state with { ShieldEffect = 6 },
            state => state.ShieldEffect <= 1);

        public static readonly Spell Poison = new(173, state => state with { PoisonEffect = 6 },
            state => state.PoisonEffect <= 1);

        public static readonly Spell Recharge = new(229, state => state with { RechargeEffect = 5 },
            state => state.RechargeEffect <= 1);

        public static readonly ImmutableArray<Spell> AllActions =
            ImmutableArray.Create(MagicMissile, Drain, Shield, Poison, Recharge);
    }

    private record struct BossData(int HitPoints, int Damage)
    {
    }
}

[Problem("2015-22-01")]
public class Day22_Part1
{
    public int Execute(string input)
    {
        return Day22_Common.Execute(input, false);
    }
}

[Problem("2015-22-02")]
public class Day22_Part2
{
    public int Execute(string input)
    {
        return Day22_Common.Execute(input, true);
    }
}