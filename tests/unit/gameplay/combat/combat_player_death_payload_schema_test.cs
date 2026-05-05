#nullable enable

using System;
using System.Linq;
using System.Reflection;
using Gravenspire.Gameplay.Combat;
using NUnit.Framework;

namespace Gravenspire.Tests.Unit.Gameplay.Combat;

public sealed class CombatPlayerDeathPayloadSchemaTest
{
    [Test]
    public void test_player_death_event_payload_schema_contains_exactly_approved_six_fields()
    {
        var properties = DeclaredPropertyNames(typeof(PlayerDeathEvent));

        Assert.That(properties, Is.EqualTo(new[]
        {
            "death_context_id",
            "death_position",
            "death_cause_type",
            "killer_source_ref",
            "local_character_id",
            "zoneId"
        }.OrderBy(name => name, StringComparer.Ordinal).ToArray()));

        Assert.That(properties, Has.No.Member("combat_" + "actor_id"));
        Assert.That(properties, Has.No.Member("account_id"));
        Assert.That(properties, Has.No.Member("p" + "vp_source"));
        Assert.That(properties, Has.No.Member("ser" + "ver_" + "authority"));
        Assert.That(properties, Has.No.Member("raw_threat_table"));
        Assert.That(properties, Has.No.Member("corpse_record"));
        Assert.That(properties, Has.No.Member("xp_penalty"));
        Assert.That(properties, Has.No.Member("xp_loss"));
        Assert.That(properties, Has.No.Member("item_drop"));
        Assert.That(properties, Has.No.Member("llm_narrative_context"));
        Assert.That(properties, Has.No.Member("tick_id"));
    }

    [Test]
    public void test_player_kill_credit_event_contract_remains_approved_four_fields()
    {
        var properties = DeclaredPropertyNames(typeof(PlayerKillCreditEvent));

        Assert.That(properties, Is.EqualTo(new[]
        {
            "defeated_source_ref",
            "faction_id",
            "kill_weight_seed",
            "zoneId"
        }.OrderBy(name => name, StringComparer.Ordinal).ToArray()));
    }

    private static string[] DeclaredPropertyNames(Type type)
    {
        return type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
    }
}
