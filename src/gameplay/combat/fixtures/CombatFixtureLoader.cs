#nullable enable

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gravenspire.Gameplay.Combat.Fixtures;

/// <summary>
/// Loads T1 Combat Core fixture data from JSON without depending on Unity asset APIs.
/// </summary>
public sealed class CombatFixtureLoader
{
    private static readonly JsonSerializerOptions JsonOptions = CreateOptions();

    /// <summary>
    /// Loads a combat fixture package from a JSON file.
    /// </summary>
    public CombatFixturePackage LoadFromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Fixture file path is required.", nameof(filePath));
        }

        var json = File.ReadAllText(filePath);
        return LoadFromJson(json);
    }

    /// <summary>
    /// Loads a combat fixture package from JSON text.
    /// </summary>
    public CombatFixturePackage LoadFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidDataException("Combat fixture JSON is empty.");
        }

        var package = JsonSerializer.Deserialize<CombatFixturePackage>(json, JsonOptions);
        if (package is null)
        {
            throw new InvalidDataException("Combat fixture JSON did not produce a package.");
        }

        return package;
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Disallow,
            AllowTrailingCommas = false
        };

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
