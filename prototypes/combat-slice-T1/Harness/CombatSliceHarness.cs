#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Gravenspire.Gameplay.Combat;
using Gravenspire.Gameplay.Combat.Fixtures;

namespace Gravenspire.Prototypes.CombatSliceT1;

internal static class CombatSliceHarness
{
    private const string BaseCommand = "dotnet run --project prototypes/combat-slice-T1/Harness/CombatSliceHarness.csproj";
    private const string DefaultEvidenceStoryId = "T1.5-COMBAT-05";
    private const string DefaultRunTimestamp = "2026-05-09T00:00:00-04:00";
    private const string TargetEngineVersion = "6000.3.x-headless-net8";
    private const string ZoneId = "Haunt_Prototype_T1";
    private const double FacingToleranceDegrees = 90.0d;
    private const double MeleeDistanceMeters = 1.5d;
    private const int MaxCombatSeconds = 240;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public static int Main(string[] args)
    {
        try
        {
            var options = HarnessOptions.Parse(args);
            var repoRoot = FindRepoRoot();
            var fixturePath = Path.Combine(repoRoot, "assets", "data", "combat", "t1-combat-fixtures.json");
            var package = new CombatFixtureLoader().LoadFromFile(fixturePath);
            var fixtureValidation = new CombatFixtureValidator().Validate(package);
            if (!fixtureValidation.IsValid)
            {
                throw new InvalidDataException("Fixture package failed validation: " + string.Join("; ", fixtureValidation.Errors));
            }

            var context = new HarnessContext(repoRoot, package, ReadBuildSha(repoRoot));
            var summaries = new[]
            {
                RunCombatScenario(context, ScenarioDefinition.SoloTrash()),
                RunCombatScenario(context, ScenarioDefinition.NamedSoloBlock()),
                RunCombatScenario(context, ScenarioDefinition.TwoTrashOverpull()),
                RunMedBreakScenario(context),
                RunStructuralSmoke(context)
            };

            var evidenceDirectory = Path.Combine(repoRoot, "tests", "evidence", options.EvidenceStoryId);
            Directory.CreateDirectory(evidenceDirectory);
            var outputPath = Path.Combine(evidenceDirectory, "profiled-combat-slice.jsonl");
            using (var writer = new StreamWriter(outputPath, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                foreach (var summary in summaries)
                {
                    writer.WriteLine(JsonSerializer.Serialize(ToJsonRecord(context, summary, options), JsonOptions));
                }
            }

            foreach (var summary in summaries)
            {
                Console.WriteLine(FormattableString.Invariant(
                    $"{summary.Scenario}: result={summary.Result} pulls={summary.PullsCompleted}/{summary.PullsTarget} wins={summary.Wins} losses={summary.Losses} flees={summary.Flees} deaths={summary.Deaths}"));
            }

            Console.WriteLine($"wrote {outputPath}");
            return summaries.All(summary => SummaryPassesStory(summary, options)) ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static ScenarioSummary RunCombatScenario(HarnessContext context, ScenarioDefinition definition)
    {
        var trials = new List<TrialResult>();
        for (var index = 0; index < definition.TrialCount; index++)
        {
            trials.Add(RunTrial(context, definition, index));
        }

        var wins = trials.Count(trial => trial.Outcome == TrialOutcome.Win);
        var losses = trials.Count(trial => trial.Outcome == TrialOutcome.Loss);
        var flees = trials.Count(trial => trial.Outcome == TrialOutcome.Flee);
        var timeouts = trials.Count(trial => trial.Outcome == TrialOutcome.Timeout);
        var dangerous = trials.Count(trial => trial.Outcome is TrialOutcome.Loss or TrialOutcome.Flee ||
            trial.EndingHealthRatio < definition.DangerHealthRatio ||
            trial.EndingManaRatio < definition.DangerManaRatio);
        var medBreaks = trials.Count(trial => trial.EndingHealthRatio < 0.80d || trial.EndingManaRatio < 0.60d);
        var totalCombatSeconds = trials.Sum(trial => trial.CombatSeconds);
        var meanHealthRatio = trials.Average(trial => trial.EndingHealthRatio);
        var meanManaRatio = trials.Average(trial => trial.EndingManaRatio);
        var meanEnduranceRatio = trials.Average(trial => trial.EndingEnduranceRatio);
        var meanWinHealthRatio = wins == 0 ? 0d : trials.Where(trial => trial.Outcome == TrialOutcome.Win).Average(trial => trial.EndingHealthRatio);
        var meanWinManaRatio = wins == 0 ? 0d : trials.Where(trial => trial.Outcome == TrialOutcome.Win).Average(trial => trial.EndingManaRatio);
        var meanWinEnduranceRatio = wins == 0 ? 0d : trials.Where(trial => trial.Outcome == TrialOutcome.Win).Average(trial => trial.EndingEnduranceRatio);

        var pass = definition.Kind switch
        {
            ScenarioKind.SoloTrash => wins is >= 18 and <= 20 && (meanHealthRatio < 0.80d || meanManaRatio < 0.60d),
            ScenarioKind.NamedSoloBlock => losses + flees >= 8,
            ScenarioKind.TwoTrashOverpull => dangerous >= 8,
            _ => false
        };

        return new ScenarioSummary(
            definition.ScenarioId,
            FinalState: "Complete",
            StoppedVia: timeouts == 0 ? "completion" : "timeout",
            PullsCompleted: trials.Count,
            PullsTarget: definition.TrialCount,
            totalCombatSeconds,
            TotalDowntimeSeconds: 0d,
            AvgPullSeconds: totalCombatSeconds / Math.Max(1, trials.Count),
            medBreaks,
            trials.Sum(trial => trial.AutoSwings),
            trials.Sum(trial => trial.HostileSwings),
            trials.Sum(trial => trial.SmitesChanneled),
            trials.Sum(trial => trial.HealsUsed),
            trials.Sum(trial => trial.SmiteOfAuthorityUses),
            trials.Sum(trial => trial.BashUses),
            trials.Sum(trial => trial.DefensivePrayerUses),
            trials.Sum(trial => trial.DefensivePrayerDamagePrevented),
            UnsafePulls: dangerous,
            Deaths: trials.Sum(trial => trial.Deaths),
            wins,
            losses,
            flees,
            DangerousOutcomes: dangerous,
            meanHealthRatio,
            meanManaRatio,
            meanEnduranceRatio,
            meanWinHealthRatio,
            meanWinManaRatio,
            meanWinEnduranceRatio,
            SecondsTo70Mana: null,
            RegenTicks: 0,
            StructuralMatches: null,
            Result: pass ? "pass" : "fail");
    }

    private static TrialResult RunTrial(HarnessContext context, ScenarioDefinition definition, int trialIndex)
    {
        var package = context.Package;
        var tickRate = package.CombatTickRateHz;
        var maxTick = MaxCombatSeconds * tickRate;
        var rolls = new SeededRollStream(definition.SeedBase + trialIndex);
        var melee = new CombatMeleeResolver();
        var instantResolver = new CombatInstantAbilityResolver();
        var gate = ActiveHauntGate();
        var playerFixture = ActorFixture(package, definition.PlayerFixtureId);
        var player = CreatePlayer(playerFixture)
            .WithTarget($"combat-hostile-{trialIndex}-0")
            .WithCombatState(CombatState.InCombat);
        var hostiles = CreateHostiles(context, definition, trialIndex);
        var playerAttackMachine = new CombatAttackStateMachine();
        var toggle = playerAttackMachine.ToggleOn(new CombatAttackToggleOnRequest(
            player,
            hostiles[0].Actor,
            gate,
            MeleeDistanceMeters,
            CombatTick.Zero,
            tickRate));
        if (!toggle.Succeeded)
        {
            throw new InvalidOperationException("Unable to enable player Attack: " + string.Join("; ", toggle.RejectionReasons));
        }

        var playerAttack = toggle.Snapshot;
        var hostileAttacks = hostiles.ToDictionary(
            hostile => hostile.Actor.CombatActorId,
            hostile => new CombatAttackStateSnapshot(
                CombatAttackMode.On,
                player.CombatActorId,
                checked(hostile.AggroTick + SecondsToTicks(hostile.Actor.WeaponDelaySeconds, tickRate)),
                CombatAttackTransitionPath.PlayerToggleOn),
            StringComparer.Ordinal);
        var slowCast = new PlayerSlowCastState();
        var smiteProfile = SlowSpellProfile(package, "Smite_T1_Prototype", definition.Band, player.SpellRangeMeters, requiresTarget: true);
        var smiteDamage = SpellEffectValue(package, "Smite_T1_Prototype", definition.Band);
        var healProfile = SlowSpellProfile(package, "LesserHeal_T1_Prototype", definition.Band, player.SpellRangeMeters, requiresTarget: false);
        var healValue = SpellEffectValue(package, "LesserHeal_T1_Prototype", definition.Band);
        var authorityProfile = TacticalProfile(package, "SmiteOfAuthority_T1_Prototype", definition.Band);
        var bashProfile = TacticalProfile(package, "Bash_T1_Prototype", definition.Band);
        var prayerProfile = TacticalProfile(package, "DefensivePrayer_T1_Prototype", definition.Band);
        var nextAuthorityTick = 0L;
        var nextBashTick = definition.Kind == ScenarioKind.SoloTrash ? tickRate * 2L : 0L;
        var nextPrayerTick = 0L;
        var prayerUntilTick = -1L;
        var prayerReduction = 0d;
        var metrics = TrialMetrics.Empty;
        var outcome = TrialOutcome.Timeout;
        long finalTick = maxTick;

        for (var tickIndex = 0L; tickIndex <= maxTick; tickIndex++)
        {
            var tick = ToTick(tickIndex, tickRate);
            EndRecoveryIfDue(slowCast, ref player, tick);
            CompleteSlowCastIfDue(slowCast, ref player, hostiles, tick, smiteDamage, healValue, ref metrics);

            if (!player.IsAlive)
            {
                outcome = TrialOutcome.Loss;
                finalTick = tickIndex;
                break;
            }

            if (hostiles.All(hostile => !hostile.Actor.IsAlive))
            {
                outcome = TrialOutcome.Win;
                finalTick = tickIndex;
                break;
            }

            var liveTarget = SelectLiveTarget(hostiles);
            if (liveTarget is not null && !string.Equals(player.TargetCombatActorId, liveTarget.Actor.CombatActorId, StringComparison.Ordinal))
            {
                player = player.WithTarget(liveTarget.Actor.CombatActorId);
                playerAttack = new CombatAttackStateSnapshot(
                    CombatAttackMode.On,
                    liveTarget.Actor.CombatActorId,
                    checked(tickIndex + SecondsToTicks(player.WeaponDelaySeconds, tickRate)),
                    CombatAttackTransitionPath.PlayerToggleOn);
            }

            if (ShouldFlee(definition, player, liveTarget?.Actor))
            {
                outcome = TrialOutcome.Flee;
                finalTick = tickIndex;
                break;
            }

            TryUseInstant(
                instantResolver,
                prayerProfile,
                ref player,
                target: null,
                tick,
                tickRate,
                ref nextPrayerTick,
                ref metrics,
                abilityCounter: AbilityCounter.DefensivePrayer,
                ref prayerUntilTick,
                ref prayerReduction);

            liveTarget = SelectLiveTarget(hostiles);
            if (liveTarget is not null)
            {
                if (tickIndex >= nextAuthorityTick && CanAffordInstant(player, authorityProfile))
                {
                    TryUseInstant(
                        instantResolver,
                        authorityProfile,
                        ref player,
                        liveTarget,
                        tick,
                        tickRate,
                        ref nextAuthorityTick,
                        ref metrics,
                        AbilityCounter.SmiteOfAuthority,
                        ref prayerUntilTick,
                        ref prayerReduction);
                }

                liveTarget = SelectLiveTarget(hostiles);
                if (liveTarget is not null && tickIndex >= nextBashTick && CanAffordInstant(player, bashProfile))
                {
                    TryUseInstant(
                        instantResolver,
                        bashProfile,
                        ref player,
                        liveTarget,
                        tick,
                        tickRate,
                        ref nextBashTick,
                        ref metrics,
                        AbilityCounter.Bash,
                        ref prayerUntilTick,
                        ref prayerReduction);
                }
            }

            liveTarget = SelectLiveTarget(hostiles);
            if (liveTarget is not null && slowCast.CanStart(player))
            {
                if (player.CurrentHealth <= Math.Ceiling(player.MaxHealth * definition.HealHealthRatio) &&
                    player.CurrentMana >= healProfile.ManaCost)
                {
                    slowCast.Start(player, target: null, tick, tickRate, healProfile, SlowCastKind.Heal);
                    player = slowCast.CurrentCaster;
                }
                else if (player.CurrentHealth > Math.Ceiling(player.MaxHealth * definition.MinimumHealthToStartSmiteRatio) &&
                    player.CurrentMana >= smiteProfile.ManaCost)
                {
                    slowCast.Start(player, liveTarget, tick, tickRate, smiteProfile, SlowCastKind.Smite);
                    player = slowCast.CurrentCaster;
                }
            }

            liveTarget = SelectLiveTarget(hostiles);
            if (liveTarget is not null)
            {
                var playerSwing = melee.ResolveTick(MeleeRequest(
                    player,
                    liveTarget.Actor,
                    playerAttack,
                    tick,
                    rolls,
                    gate,
                    tickRate));
                if (playerSwing.Outcome != CombatMeleeTickOutcome.NotDue)
                {
                    metrics = metrics with { AutoSwings = metrics.AutoSwings + 1 };
                    playerAttack = playerAttack with { NextSwingDueTick = playerSwing.NextSwingDueTick };
                    if (playerSwing.TargetAfterResolution is not null)
                    {
                        liveTarget.Actor = playerSwing.TargetAfterResolution;
                    }
                }
            }

            foreach (var hostile in hostiles.Where(hostile => hostile.Actor.IsAlive && tickIndex >= hostile.AggroTick))
            {
                var hostileAttack = hostileAttacks[hostile.Actor.CombatActorId];
                var hostileSwing = melee.ResolveTick(MeleeRequest(
                    hostile.Actor,
                    player,
                    hostileAttack,
                    tick,
                    rolls,
                    gate,
                    tickRate));
                if (hostileSwing.Outcome == CombatMeleeTickOutcome.NotDue)
                {
                    continue;
                }

                metrics = metrics with { HostileSwings = metrics.HostileSwings + 1 };
                hostileAttacks[hostile.Actor.CombatActorId] = hostileAttack with { NextSwingDueTick = hostileSwing.NextSwingDueTick };
                if (hostileSwing.Damage > 0)
                {
                    var actualDamage = hostileSwing.Damage;
                    if (tickIndex <= prayerUntilTick && prayerReduction > 0d)
                    {
                        var prevented = (int)Math.Floor(hostileSwing.Damage * prayerReduction);
                        actualDamage -= prevented;
                        metrics = metrics with { DefensivePrayerDamagePrevented = metrics.DefensivePrayerDamagePrevented + prevented };
                    }

                    player = ApplyDamage(player, actualDamage);
                    TryInterruptSlowCast(slowCast, ref player, liveTarget?.Actor, tick, actualDamage, rolls);
                }
            }
        }

        var deaths = outcome == TrialOutcome.Loss ? 1 : 0;
        return new TrialResult(
            outcome,
            finalTick / (double)tickRate,
            EndingHealthRatio: player.CurrentHealth / (double)player.MaxHealth,
            EndingManaRatio: player.MaxMana == 0 ? 0d : player.CurrentMana / (double)player.MaxMana,
            EndingEnduranceRatio: player.MaxEndurance == 0 ? 0d : player.CurrentEndurance / (double)player.MaxEndurance,
            metrics.AutoSwings,
            metrics.HostileSwings,
            metrics.SmitesChanneled,
            metrics.HealsUsed,
            metrics.SmiteOfAuthorityUses,
            metrics.BashUses,
            metrics.DefensivePrayerUses,
            metrics.DefensivePrayerDamagePrevented,
            deaths);
    }

    private static ScenarioSummary RunMedBreakScenario(HarnessContext context)
    {
        var package = context.Package;
        var tickRate = package.CombatTickRateHz;
        var fixture = ActorFixture(package, "Cleric_Mid_T1");
        var player = CreatePlayer(fixture, currentMana: 30)
            .WithCombatState(CombatState.OutOfCombat) with
        {
            PostureState = CombatPostureState.Sitting,
            NextRegenTickIndex = SecondsToTicks(package.RegenAndCombatExitTuning.RegenTickIntervalSeconds, tickRate)
        };
        var regen = new CombatRegenResolver();
        var targetMana = (int)Math.Ceiling(player.MaxMana * 0.70d);
        var regenTicks = 0;
        var secondsToTarget = 0d;

        for (var tickIndex = 0L; tickIndex <= 180 * tickRate; tickIndex++)
        {
            var result = regen.ResolveTick(new CombatRegenTickRequest(
                player,
                package.RegenAndCombatExitTuning,
                ToTick(tickIndex, tickRate),
                tickRate));
            player = result.Actor;
            if (result.TickResolved)
            {
                regenTicks++;
            }

            if (player.CurrentMana >= targetMana)
            {
                secondsToTarget = tickIndex / (double)tickRate;
                break;
            }
        }

        var pass = secondsToTarget is >= 60d and <= 120d && regenTicks > 0;
        return new ScenarioSummary(
            "MedBreak_Pacing_T1",
            "Complete",
            "completion",
            PullsCompleted: 1,
            PullsTarget: 1,
            TotalCombatSeconds: 0d,
            TotalDowntimeSeconds: secondsToTarget,
            AvgPullSeconds: 0d,
            MedBreaks: 1,
            AutoSwings: 0,
            HostileSwings: 0,
            SmitesChanneled: 0,
            HealsUsed: 0,
            SmiteOfAuthorityUses: 0,
            BashUses: 0,
            DefensivePrayerUses: 0,
            DefensivePrayerDamagePrevented: 0,
            UnsafePulls: 0,
            Deaths: 0,
            Wins: 1,
            Losses: 0,
            Flees: 0,
            DangerousOutcomes: 0,
            MeanEndingHealthRatio: player.CurrentHealth / (double)player.MaxHealth,
            MeanEndingManaRatio: player.CurrentMana / (double)player.MaxMana,
            MeanEndingEnduranceRatio: player.MaxEndurance == 0 ? 0d : player.CurrentEndurance / (double)player.MaxEndurance,
            MeanWinEndingHealthRatio: player.CurrentHealth / (double)player.MaxHealth,
            MeanWinEndingManaRatio: player.CurrentMana / (double)player.MaxMana,
            MeanWinEndingEnduranceRatio: player.MaxEndurance == 0 ? 0d : player.CurrentEndurance / (double)player.MaxEndurance,
            SecondsTo70Mana: secondsToTarget,
            RegenTicks: regenTicks,
            StructuralMatches: null,
            Result: pass ? "pass" : "fail");
    }

    private static ScenarioSummary RunStructuralSmoke(HarnessContext context)
    {
        var sourceDirectory = Path.Combine(context.RepoRoot, "src", "gameplay", "combat");
        var forbidden = new[] { "Camera.", "AudioSource", "Animator", "MonoBehaviour", "UnityEngine.UI", "UnityEngine.UIElements" };
        var matches = 0;
        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            matches += forbidden.Count(item => text.Contains(item, StringComparison.Ordinal));
        }

        return new ScenarioSummary(
            "DevBuild_StructuralSmoke_T1",
            "Complete",
            "structural_scan",
            PullsCompleted: 1,
            PullsTarget: 1,
            TotalCombatSeconds: 0d,
            TotalDowntimeSeconds: 0d,
            AvgPullSeconds: 0d,
            MedBreaks: 0,
            AutoSwings: 0,
            HostileSwings: 0,
            SmitesChanneled: 0,
            HealsUsed: 0,
            SmiteOfAuthorityUses: 0,
            BashUses: 0,
            DefensivePrayerUses: 0,
            DefensivePrayerDamagePrevented: 0,
            UnsafePulls: matches,
            Deaths: 0,
            Wins: 1,
            Losses: 0,
            Flees: 0,
            DangerousOutcomes: matches,
            MeanEndingHealthRatio: 1d,
            MeanEndingManaRatio: 1d,
            MeanEndingEnduranceRatio: 1d,
            MeanWinEndingHealthRatio: 1d,
            MeanWinEndingManaRatio: 1d,
            MeanWinEndingEnduranceRatio: 1d,
            SecondsTo70Mana: null,
            RegenTicks: 0,
            StructuralMatches: matches,
            Result: matches == 0 ? "pass" : "fail");
    }

    private static Dictionary<string, object?> ToJsonRecord(HarnessContext context, ScenarioSummary summary, HarnessOptions options)
    {
        return new Dictionary<string, object?>
        {
            ["timestamp"] = options.RunTimestamp,
            ["engine_version"] = TargetEngineVersion,
            ["fixture_set_version"] = context.Package.FixtureSetVersion,
            ["build_sha"] = context.BuildSha,
            ["test_scenario"] = summary.Scenario,
            ["final_state"] = summary.FinalState,
            ["stopped_via"] = summary.StoppedVia,
            ["pulls_completed"] = summary.PullsCompleted,
            ["pulls_target"] = summary.PullsTarget,
            ["total_combat_seconds"] = Round(summary.TotalCombatSeconds),
            ["total_downtime_seconds"] = Round(summary.TotalDowntimeSeconds),
            ["avg_pull_seconds"] = Round(summary.AvgPullSeconds),
            ["med_breaks"] = summary.MedBreaks,
            ["auto_swings"] = summary.AutoSwings,
            ["hostile_swings"] = summary.HostileSwings,
            ["smites_channeled"] = summary.SmitesChanneled,
            ["heals_used"] = summary.HealsUsed,
            ["smite_of_authority_uses"] = summary.SmiteOfAuthorityUses,
            ["bash_uses"] = summary.BashUses,
            ["defensive_prayer_uses"] = summary.DefensivePrayerUses,
            ["defensive_prayer_damage_prevented"] = summary.DefensivePrayerDamagePrevented,
            ["unsafe_pulls"] = summary.UnsafePulls,
            ["deaths"] = summary.Deaths,
            ["command"] = options.Command,
            ["result"] = summary.Result,
            ["wins"] = summary.Wins,
            ["losses"] = summary.Losses,
            ["flees"] = summary.Flees,
            ["dangerous_outcomes"] = summary.DangerousOutcomes,
            ["mean_ending_health_ratio"] = Round(summary.MeanEndingHealthRatio),
            ["mean_ending_mana_ratio"] = Round(summary.MeanEndingManaRatio),
            ["mean_ending_endurance_ratio"] = Round(summary.MeanEndingEnduranceRatio),
            ["mean_win_ending_health_ratio"] = Round(summary.MeanWinEndingHealthRatio),
            ["mean_win_ending_mana_ratio"] = Round(summary.MeanWinEndingManaRatio),
            ["mean_win_ending_endurance_ratio"] = Round(summary.MeanWinEndingEnduranceRatio),
            ["seconds_to_70_mana"] = summary.SecondsTo70Mana is null ? null : Round(summary.SecondsTo70Mana.Value),
            ["regen_ticks"] = summary.RegenTicks,
            ["structural_matches"] = summary.StructuralMatches
        };
    }

    private static bool SummaryPassesStory(ScenarioSummary summary, HarnessOptions options)
    {
        if (options.AllowSoloTrashFailure &&
            string.Equals(summary.Scenario, "SoloTrash_EvenCon_T1", StringComparison.Ordinal))
        {
            return true;
        }

        return string.Equals(summary.Result, "pass", StringComparison.Ordinal);
    }

    private static void TryUseInstant(
        CombatInstantAbilityResolver resolver,
        CombatTacticalAbilityProfile profile,
        ref CombatActorState player,
        HostileState? target,
        CombatTick tick,
        int tickRate,
        ref long nextAllowedTick,
        ref TrialMetrics metrics,
        AbilityCounter abilityCounter,
        ref long prayerUntilTick,
        ref double prayerReduction)
    {
        if (tick.Index < nextAllowedTick ||
            player.CastRuntimeState != CombatCastRuntimeState.None ||
            !CanAffordInstant(player, profile))
        {
            return;
        }

        var result = resolver.Resolve(new CombatInstantAbilityRequest(
            $"activation-{profile.AbilityId}-{tick.Index.ToString(CultureInfo.InvariantCulture)}",
            player,
            target?.Actor,
            ActiveHauntGate(),
            profile.RequiresTarget ? MeleeDistanceMeters : 0d,
            Array.Empty<CombatLosLayer>(),
            tick,
            tickRate,
            profile));
        if (!result.Succeeded)
        {
            if (result.CooldownRemainingTicks is not null)
            {
                nextAllowedTick = tick.Index + result.CooldownRemainingTicks.Value;
            }

            return;
        }

        player = result.Caster;
        if (target is not null && result.TargetAfterResolution is not null)
        {
            target.Actor = result.TargetAfterResolution;
        }

        nextAllowedTick = result.CooldownEndsTick ?? checked(tick.Index + SecondsToTicks(profile.CooldownSeconds, tickRate));
        switch (abilityCounter)
        {
            case AbilityCounter.SmiteOfAuthority:
                metrics = metrics with { SmiteOfAuthorityUses = metrics.SmiteOfAuthorityUses + 1 };
                break;
            case AbilityCounter.Bash:
                metrics = metrics with { BashUses = metrics.BashUses + 1 };
                break;
            case AbilityCounter.DefensivePrayer:
                metrics = metrics with { DefensivePrayerUses = metrics.DefensivePrayerUses + 1 };
                var buff = result.AppliedEffects.SingleOrDefault(effect => effect.EffectType == CombatTacticalAbilityEffectType.SelfBuff);
                if (buff is { DurationSeconds: not null, DamageReduction: not null })
                {
                    prayerUntilTick = checked(tick.Index + SecondsToTicks(buff.DurationSeconds.Value, tickRate));
                    prayerReduction = buff.DamageReduction.Value;
                }

                break;
        }
    }

    private static void TryInterruptSlowCast(
        PlayerSlowCastState slowCast,
        ref CombatActorState player,
        CombatActorState? target,
        CombatTick tick,
        int damageTaken,
        SeededRollStream rolls)
    {
        if (!slowCast.IsCasting || damageTaken <= 0)
        {
            return;
        }

        var interrupted = slowCast.Machine!.InterruptFromDamage(
            player,
            target,
            new CombatDamageInterruptRequest(
                damageTaken,
                DamageWasBlockedOrAbsorbed: false,
                tick,
                InterruptTuning(),
                rolls));
        player = interrupted.Caster;
        if (interrupted.Outcome == CombatCastTransitionOutcome.Interrupted)
        {
            var recovery = slowCast.Machine.BeginRecoveryAfterInterrupt(player, tick);
            player = recovery.Caster;
            slowCast.ClearAction();
        }
    }

    private static void CompleteSlowCastIfDue(
        PlayerSlowCastState slowCast,
        ref CombatActorState player,
        IReadOnlyList<HostileState> hostiles,
        CombatTick tick,
        int smiteDamage,
        int healValue,
        ref TrialMetrics metrics)
    {
        if (!slowCast.IsCasting || slowCast.Machine!.CurrentCast is not { } active || tick.Index < active.CompletionTick)
        {
            return;
        }

        var target = slowCast.TargetCombatActorId is null
            ? null
            : hostiles.SingleOrDefault(hostile => string.Equals(hostile.Actor.CombatActorId, slowCast.TargetCombatActorId, StringComparison.Ordinal));
        var completed = slowCast.Machine.ResolveCompletion(player, target?.Actor, tick);
        if (completed.Outcome != CombatCastTransitionOutcome.Completed)
        {
            slowCast.ClearAll();
            return;
        }

        player = completed.Caster;
        if (slowCast.Kind == SlowCastKind.Smite && target is not null)
        {
            target.Actor = target.Actor.WithCurrentHealthAfterAbilityDamage(smiteDamage);
            metrics = metrics with { SmitesChanneled = metrics.SmitesChanneled + 1 };
        }
        else if (slowCast.Kind == SlowCastKind.Heal)
        {
            player = Heal(player, healValue);
            metrics = metrics with { HealsUsed = metrics.HealsUsed + 1 };
        }

        slowCast.ClearAction();
    }

    private static void EndRecoveryIfDue(PlayerSlowCastState slowCast, ref CombatActorState player, CombatTick tick)
    {
        if (slowCast.Machine?.CurrentCast?.RecoveryEndTick is not { } recoveryEndTick || tick.Index < recoveryEndTick)
        {
            return;
        }

        var ended = slowCast.Machine.EndRecovery(player, tick);
        if (ended.Outcome == CombatCastTransitionOutcome.RecoveryEnded)
        {
            player = ended.Caster.WithCombatState(CombatState.InCombat);
            slowCast.ClearAll();
        }
    }

    private static bool ShouldFlee(ScenarioDefinition definition, CombatActorState player, CombatActorState? target)
    {
        if (target is null || !target.IsAlive)
        {
            return false;
        }

        return player.CurrentHealth / (double)player.MaxHealth <= definition.FleeHealthRatio &&
            player.CurrentMana / (double)Math.Max(1, player.MaxMana) <= definition.FleeManaRatio;
    }

    private static HostileState? SelectLiveTarget(IEnumerable<HostileState> hostiles)
    {
        return hostiles.FirstOrDefault(hostile => hostile.Actor.IsAlive);
    }

    private static CombatMeleeTickRequest MeleeRequest(
        CombatActorState attacker,
        CombatActorState target,
        CombatAttackStateSnapshot attackState,
        CombatTick tick,
        SeededRollStream rolls,
        CombatZoneGate gate,
        int tickRate)
    {
        return new CombatMeleeTickRequest(
            attacker,
            target,
            attackState,
            gate,
            MeleeDistanceMeters,
            FacingDegreesToTarget: 0.0d,
            FacingToleranceDegrees,
            Array.Empty<CombatLosLayer>(),
            tick,
            tickRate,
            HitTuning(),
            DamageTuning(),
            rolls);
    }

    private static IReadOnlyList<HostileState> CreateHostiles(HarnessContext context, ScenarioDefinition definition, int trialIndex)
    {
        var hostiles = new List<HostileState>();
        for (var index = 0; index < definition.HostileFixtureIds.Count; index++)
        {
            var fixture = ActorFixture(context.Package, definition.HostileFixtureIds[index]);
            var actor = CreateNpc(fixture, $"combat-hostile-{trialIndex}-{index}", $"hostile-{trialIndex}-{index:D2}")
                .ClaimHostile("combat-player-1", context.Package.PullTuning.ProximityThreatInitial, CombatState.InCombat);
            hostiles.Add(new HostileState(actor, definition.HostileAggroTicks[index]));
        }

        return new ReadOnlyCollection<HostileState>(hostiles);
    }

    private static CombatActorState CreatePlayer(CombatActorFixture fixture, int? currentMana = null)
    {
        return new CombatActorState(
            "combat-player-1",
            fixture.ActorKind,
            CombatStableSourceRef.ForPlayer("local-character-1"),
            fixture.FactionId,
            ZoneId,
            fixture.Level,
            fixture.MaxHealth,
            fixture.MaxHealth,
            fixture.MaxMana,
            currentMana ?? fixture.MaxMana,
            fixture.ArmorClass,
            fixture.AttackPower,
            fixture.WeaponBaseDamage,
            fixture.AttackSkill,
            fixture.DefenseSkill,
            fixture.WeaponDelaySeconds,
            fixture.MeleeRangeMeters,
            fixture.SpellRangeMeters,
            CombatState.OutOfCombat,
            CombatActorLifeState.Alive,
            null,
            "player-local-character-1",
            threatTable: null,
            maxEndurance: fixture.MaxEndurance,
            currentEndurance: fixture.MaxEndurance);
    }

    private static CombatActorState CreateNpc(CombatActorFixture fixture, string combatActorId, string sortKey)
    {
        return new CombatActorState(
            combatActorId,
            fixture.ActorKind,
            CombatStableSourceRef.ForSpawn(new CombatSpawnSourceRef("VampireCourt_T1", $"{combatActorId}-anchor", fixture.Id)),
            fixture.FactionId,
            ZoneId,
            fixture.Level,
            fixture.MaxHealth,
            fixture.MaxHealth,
            fixture.MaxMana,
            fixture.MaxMana,
            fixture.ArmorClass,
            fixture.AttackPower,
            fixture.WeaponBaseDamage,
            fixture.AttackSkill,
            fixture.DefenseSkill,
            fixture.WeaponDelaySeconds,
            fixture.MeleeRangeMeters,
            fixture.SpellRangeMeters,
            CombatState.OutOfCombat,
            CombatActorLifeState.Alive,
            null,
            sortKey,
            threatTable: null,
            maxEndurance: fixture.MaxEndurance,
            currentEndurance: fixture.MaxEndurance);
    }

    private static CombatActorState ApplyDamage(CombatActorState actor, int damage)
    {
        return actor.WithCurrentHealthAfterAbilityDamage(Math.Max(0, damage));
    }

    private static CombatActorState Heal(CombatActorState actor, int amount)
    {
        return CopyActor(
            actor,
            currentHealth: Math.Min(actor.MaxHealth, checked(actor.CurrentHealth + amount)),
            currentMana: actor.CurrentMana,
            combatState: actor.CombatState,
            lifeState: actor.LifeState);
    }

    private static CombatActorState CopyActor(
        CombatActorState actor,
        int currentHealth,
        int currentMana,
        CombatState combatState,
        CombatActorLifeState lifeState)
    {
        return new CombatActorState(
            actor.CombatActorId,
            actor.ActorKind,
            actor.StableSourceRef,
            actor.FactionId,
            actor.ZoneId,
            actor.Level,
            actor.MaxHealth,
            currentHealth,
            actor.MaxMana,
            currentMana,
            actor.ArmorClass,
            actor.AttackPower,
            actor.WeaponBaseDamage,
            actor.AttackSkill,
            actor.DefenseSkill,
            actor.WeaponDelaySeconds,
            actor.MeleeRangeMeters,
            actor.SpellRangeMeters,
            combatState,
            lifeState,
            actor.TargetCombatActorId,
            actor.CombatSortKey,
            actor.ThreatTable,
            maxEndurance: actor.MaxEndurance,
            currentEndurance: actor.CurrentEndurance) with
        {
            CastRuntimeState = actor.CastRuntimeState,
            ActiveCastId = actor.ActiveCastId,
            ActiveCastSpellId = actor.ActiveCastSpellId,
            ActiveCastTargetCombatActorId = actor.ActiveCastTargetCombatActorId,
            CastProgressSeconds = actor.CastProgressSeconds,
            CastRecoveryRemainingSeconds = actor.CastRecoveryRemainingSeconds,
            PostureState = actor.PostureState,
            NextRegenTickIndex = actor.NextRegenTickIndex,
            LastHostileActionTickIndex = actor.LastHostileActionTickIndex,
            CombatExitRemainingSeconds = actor.CombatExitRemainingSeconds
        };
    }

    private static CombatCastProfile SlowSpellProfile(
        CombatFixturePackage package,
        string spellId,
        string band,
        double rangeMeters,
        bool requiresTarget)
    {
        var fixture = package.SpellFixtures.Single(spell => string.Equals(spell.Id, spellId, StringComparison.Ordinal));
        return new CombatCastProfile(
            fixture.Id,
            fixture.CastTimeSeconds,
            BandValue(fixture.ManaCostByBand, band, fixture.Id),
            rangeMeters,
            fixture.RecoverySeconds,
            RequiresTarget: requiresTarget,
            RequiresLineOfSight: requiresTarget);
    }

    private static int SpellEffectValue(CombatFixturePackage package, string spellId, string band)
    {
        var fixture = package.SpellFixtures.Single(spell => string.Equals(spell.Id, spellId, StringComparison.Ordinal));
        return BandValue(fixture.EffectValueByBand, band, fixture.Id);
    }

    private static CombatTacticalAbilityProfile TacticalProfile(CombatFixturePackage package, string abilityId, string band)
    {
        var fixture = package.TacticalInstantAbilityProfiles.Single(profile => string.Equals(profile.Id, abilityId, StringComparison.Ordinal));
        return CombatTacticalAbilityProfile.FromFixture(fixture, band);
    }

    private static bool CanAffordInstant(CombatActorState player, CombatTacticalAbilityProfile profile)
    {
        return player.CurrentMana >= profile.CostMana &&
            player.CurrentEndurance >= profile.CostEndurance;
    }

    private static int BandValue(IEnumerable<CombatBandValue> values, string band, string id)
    {
        foreach (var value in values)
        {
            if (string.Equals(value.Band, band, StringComparison.Ordinal))
            {
                return value.Value;
            }
        }

        throw new InvalidDataException($"{id} is missing band {band}.");
    }

    private static CombatActorFixture ActorFixture(CombatFixturePackage package, string fixtureId)
    {
        return package.ActorFixtures.Single(fixture => string.Equals(fixture.Id, fixtureId, StringComparison.Ordinal));
    }

    private static CombatZoneGate ActiveHauntGate()
    {
        var gate = new CombatZoneGate();
        gate.ActivateZone(ZoneId, CombatZoneType.HauntZone);
        return gate;
    }

    private static CombatMeleeHitChanceTuning HitTuning()
    {
        return new CombatMeleeHitChanceTuning(0.72d, 0.03d, 0.001d, 0.10d, 0.92d);
    }

    private static CombatMeleeDamageTuning DamageTuning()
    {
        return new CombatMeleeDamageTuning(0.20d, 0.10d);
    }

    private static CombatInterruptFormulaTuning InterruptTuning()
    {
        return new CombatInterruptFormulaTuning(0.20d, 4.0d, 0.10d, 0.05d, 0.85d);
    }

    private static CombatTick ToTick(long tickIndex, int tickRate)
    {
        return new CombatTick(tickIndex, tickIndex / (double)tickRate);
    }

    private static long SecondsToTicks(double seconds, int tickRate)
    {
        return Math.Max(1L, checked((long)Math.Ceiling(seconds * tickRate)));
    }

    private static double Round(double value)
    {
        return Math.Round(value, 3, MidpointRounding.AwayFromZero);
    }

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")) &&
                Directory.Exists(Path.Combine(directory.FullName, "assets")) &&
                Directory.Exists(Path.Combine(directory.FullName, "src")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Unable to locate Gravenspire repository root.");
    }

    private static string ReadBuildSha(string repoRoot)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "rev-parse --short HEAD",
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
        return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output) ? output : "unknown";
    }

    private sealed class PlayerSlowCastState
    {
        public CombatCastStateMachine? Machine { get; private set; }

        public SlowCastKind? Kind { get; private set; }

        public string? TargetCombatActorId { get; private set; }

        public CombatActorState CurrentCaster { get; private set; } = null!;

        public bool IsCasting => Machine is not null && Kind is not null && CurrentCaster.CastRuntimeState == CombatCastRuntimeState.Casting;

        public bool CanStart(CombatActorState player)
        {
            return Machine is null && player.CastRuntimeState == CombatCastRuntimeState.None && player.IsAlive;
        }

        public void Start(
            CombatActorState player,
            HostileState? target,
            CombatTick tick,
            int tickRate,
            CombatCastProfile profile,
            SlowCastKind kind)
        {
            Machine = new CombatCastStateMachine();
            Kind = kind;
            TargetCombatActorId = target?.Actor.CombatActorId;
            var result = Machine.StartCast(new CombatCastStartRequest(
                $"cast-{profile.SpellId}-{tick.Index.ToString(CultureInfo.InvariantCulture)}",
                player,
                target?.Actor,
                ActiveHauntGate(),
                profile.RequiresTarget ? 20.0d : 0d,
                Array.Empty<CombatLosLayer>(),
                tick,
                tickRate,
                profile));
            if (!result.Succeeded)
            {
                ClearAll();
                CurrentCaster = player;
                return;
            }

            CurrentCaster = result.Caster;
        }

        public void ClearAction()
        {
            Kind = null;
            TargetCombatActorId = null;
        }

        public void ClearAll()
        {
            Machine = null;
            Kind = null;
            TargetCombatActorId = null;
        }
    }

    private sealed class SeededRollStream : ICombatMeleeRandomSource, ICombatCastInterruptRandomSource
    {
        private uint state;

        public SeededRollStream(int seed)
        {
            state = unchecked((uint)seed);
            if (state == 0)
            {
                state = 0x9E3779B9u;
            }
        }

        public double NextHitRoll()
        {
            return NextRatio();
        }

        public double NextDamageRollScalar()
        {
            return 0.85d + (NextRatio() * 0.30d);
        }

        public double NextInterruptRoll()
        {
            return NextRatio();
        }

        private double NextRatio()
        {
            state = unchecked((state * 1664525u) + 1013904223u);
            return (state & 0x00FFFFFFu) / (double)0x01000000u;
        }
    }

    private sealed record HarnessOptions(string EvidenceStoryId, string RunTimestamp, bool HasExplicitOptions)
    {
        public bool AllowSoloTrashFailure =>
            string.Equals(EvidenceStoryId, "T1.5-COMBAT-03", StringComparison.Ordinal);

        public string Command
        {
            get
            {
                if (!HasExplicitOptions &&
                    string.Equals(EvidenceStoryId, DefaultEvidenceStoryId, StringComparison.Ordinal) &&
                    string.Equals(RunTimestamp, DefaultRunTimestamp, StringComparison.Ordinal))
                {
                    return BaseCommand;
                }

                return FormattableString.Invariant(
                    $"{BaseCommand} -- --evidence-story {EvidenceStoryId} --timestamp {RunTimestamp}");
            }
        }

        public static HarnessOptions Parse(IReadOnlyList<string> args)
        {
            var evidenceStoryId = DefaultEvidenceStoryId;
            var runTimestamp = DefaultRunTimestamp;
            var hasExplicitOptions = false;

            for (var index = 0; index < args.Count; index++)
            {
                var arg = args[index];
                if (string.Equals(arg, "--evidence-story", StringComparison.Ordinal))
                {
                    hasExplicitOptions = true;
                    evidenceStoryId = RequireValue(args, ref index, arg);
                    continue;
                }

                if (string.Equals(arg, "--timestamp", StringComparison.Ordinal))
                {
                    hasExplicitOptions = true;
                    runTimestamp = RequireValue(args, ref index, arg);
                    continue;
                }

                throw new ArgumentException($"Unknown harness argument: {arg}");
            }

            return new HarnessOptions(evidenceStoryId, runTimestamp, hasExplicitOptions);
        }

        private static string RequireValue(IReadOnlyList<string> args, ref int index, string arg)
        {
            if (index + 1 >= args.Count || string.IsNullOrWhiteSpace(args[index + 1]))
            {
                throw new ArgumentException($"{arg} requires a non-empty value.");
            }

            index++;
            return args[index];
        }
    }

    private sealed record HarnessContext(string RepoRoot, CombatFixturePackage Package, string BuildSha);

    private sealed record ScenarioDefinition(
        string ScenarioId,
        ScenarioKind Kind,
        string PlayerFixtureId,
        IReadOnlyList<string> HostileFixtureIds,
        IReadOnlyList<long> HostileAggroTicks,
        string Band,
        int TrialCount,
        int SeedBase,
        double HealHealthRatio,
        double MinimumHealthToStartSmiteRatio,
        double FleeHealthRatio,
        double FleeManaRatio,
        double DangerHealthRatio,
        double DangerManaRatio)
    {
        public static ScenarioDefinition SoloTrash()
        {
            return new ScenarioDefinition(
                "SoloTrash_EvenCon_T1",
                ScenarioKind.SoloTrash,
                "Cleric_Mid_T1",
                new[] { "Trash_Mid_T1" },
                new[] { 0L },
                "Mid",
                TrialCount: 20,
                SeedBase: 10101,
                HealHealthRatio: 0.48d,
                MinimumHealthToStartSmiteRatio: 0.38d,
                FleeHealthRatio: 0.08d,
                FleeManaRatio: 0.05d,
                DangerHealthRatio: 0.20d,
                DangerManaRatio: 0.10d);
        }

        public static ScenarioDefinition NamedSoloBlock()
        {
            return new ScenarioDefinition(
                "NamedSoloBlock_T1",
                ScenarioKind.NamedSoloBlock,
                "Cleric_Top_T1",
                new[] { "Named_Top_T1" },
                new[] { 0L },
                "Top",
                TrialCount: 10,
                SeedBase: 20202,
                HealHealthRatio: 0.58d,
                MinimumHealthToStartSmiteRatio: 0.40d,
                FleeHealthRatio: 0.22d,
                FleeManaRatio: 0.16d,
                DangerHealthRatio: 0.20d,
                DangerManaRatio: 0.10d);
        }

        public static ScenarioDefinition TwoTrashOverpull()
        {
            return new ScenarioDefinition(
                "TwoTrash_Overpull_T1",
                ScenarioKind.TwoTrashOverpull,
                "Cleric_Mid_T1",
                new[] { "Trash_Mid_Overpull_T1", "Trash_Mid_Overpull_T1" },
                new[] { 0L, 0L },
                "Mid",
                TrialCount: 10,
                SeedBase: 30303,
                HealHealthRatio: 0.52d,
                MinimumHealthToStartSmiteRatio: 0.42d,
                FleeHealthRatio: 0.18d,
                FleeManaRatio: 0.12d,
                DangerHealthRatio: 0.20d,
                DangerManaRatio: 0.10d);
        }
    }

    private sealed record ScenarioSummary(
        string Scenario,
        string FinalState,
        string StoppedVia,
        int PullsCompleted,
        int PullsTarget,
        double TotalCombatSeconds,
        double TotalDowntimeSeconds,
        double AvgPullSeconds,
        int MedBreaks,
        int AutoSwings,
        int HostileSwings,
        int SmitesChanneled,
        int HealsUsed,
        int SmiteOfAuthorityUses,
        int BashUses,
        int DefensivePrayerUses,
        int DefensivePrayerDamagePrevented,
        int UnsafePulls,
        int Deaths,
        int Wins,
        int Losses,
        int Flees,
        int DangerousOutcomes,
        double MeanEndingHealthRatio,
        double MeanEndingManaRatio,
        double MeanEndingEnduranceRatio,
        double MeanWinEndingHealthRatio,
        double MeanWinEndingManaRatio,
        double MeanWinEndingEnduranceRatio,
        double? SecondsTo70Mana,
        int RegenTicks,
        int? StructuralMatches,
        string Result);

    private sealed record TrialResult(
        TrialOutcome Outcome,
        double CombatSeconds,
        double EndingHealthRatio,
        double EndingManaRatio,
        double EndingEnduranceRatio,
        int AutoSwings,
        int HostileSwings,
        int SmitesChanneled,
        int HealsUsed,
        int SmiteOfAuthorityUses,
        int BashUses,
        int DefensivePrayerUses,
        int DefensivePrayerDamagePrevented,
        int Deaths);

    private sealed record TrialMetrics(
        int AutoSwings,
        int HostileSwings,
        int SmitesChanneled,
        int HealsUsed,
        int SmiteOfAuthorityUses,
        int BashUses,
        int DefensivePrayerUses,
        int DefensivePrayerDamagePrevented)
    {
        public static TrialMetrics Empty { get; } = new(0, 0, 0, 0, 0, 0, 0, 0);
    }

    private sealed class HostileState
    {
        public HostileState(CombatActorState actor, long aggroTick)
        {
            Actor = actor;
            AggroTick = aggroTick;
        }

        public CombatActorState Actor { get; set; }

        public long AggroTick { get; }
    }

    private enum TrialOutcome
    {
        Win,
        Loss,
        Flee,
        Timeout
    }

    private enum ScenarioKind
    {
        SoloTrash,
        NamedSoloBlock,
        TwoTrashOverpull
    }

    private enum SlowCastKind
    {
        Smite,
        Heal
    }

    private enum AbilityCounter
    {
        SmiteOfAuthority,
        Bash,
        DefensivePrayer
    }
}
