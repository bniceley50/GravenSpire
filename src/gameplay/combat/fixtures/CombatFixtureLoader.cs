#nullable enable

using System;
using System.IO;
#if UNITY_5_3_OR_NEWER
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
#else
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace Gravenspire.Gameplay.Combat.Fixtures;

/// <summary>
/// Loads T1 Combat Core fixture data from JSON without depending on Unity asset APIs.
/// </summary>
public sealed class CombatFixtureLoader
{
#if UNITY_5_3_OR_NEWER
    private static readonly JsonSerializerSettings JsonSettings = CreateSettings();
#else
    private static readonly JsonSerializerOptions JsonOptions = CreateOptions();
#endif

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

        CombatFixturePackage? package;
        try
        {
#if UNITY_5_3_OR_NEWER
            package = JsonConvert.DeserializeObject<CombatFixturePackage>(json, JsonSettings);
#else
            package = JsonSerializer.Deserialize<CombatFixturePackage>(json, JsonOptions);
#endif
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("Combat fixture JSON could not be parsed.", ex);
        }

        if (package is null)
        {
            throw new InvalidDataException("Combat fixture JSON did not produce a package.");
        }

        return package;
    }

#if UNITY_5_3_OR_NEWER
    private static JsonSerializerSettings CreateSettings()
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        settings.Converters.Add(new StringEnumConverter
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        });

        return settings;
    }
#else
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
#endif
}
