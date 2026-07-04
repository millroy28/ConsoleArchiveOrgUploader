using System.Text.Json;

namespace ArchiveOrgUploader;

/// <summary>
/// Manages the "last used settings" file (usersettings.json), separate from appsettings.json.
/// appsettings.json is treated as the app's shipped/template defaults and is never modified by
/// the app itself. usersettings.json is created next to the exe on first run (seeded from
/// appsettings.json) and is what gets loaded — and re-saved — on every run after that, so any
/// changes made in the settings menu carry forward automatically.
/// </summary>
public static class SettingsManager
{
    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    public static Config LoadOrCreate(string appSettingsPath, string userSettingsPath, ConsoleWriter console, FileLogger logger)
    {
        if (File.Exists(userSettingsPath))
        {
            logger.Info($"Loading settings from '{userSettingsPath}'.");
            try
            {
                var loaded = JsonSerializer.Deserialize<Config>(File.ReadAllText(userSettingsPath));
                if (loaded != null)
                    return loaded;
            }
            catch (JsonException ex)
            {
                console.PrintWarn($"Could not parse '{userSettingsPath}' ({ex.Message}); falling back to appsettings.json defaults.");
                logger.Warn($"Could not parse '{userSettingsPath}': {ex.Message}. Falling back to '{appSettingsPath}'.");
            }
        }

        // First run (or a corrupt/missing usersettings.json): seed from the shipped template.
        logger.Info($"No usable usersettings file found. Bootstrapping from '{appSettingsPath}'.");
        var defaults = JsonSerializer.Deserialize<Config>(File.ReadAllText(appSettingsPath))
                       ?? throw new Exception($"Could not parse {appSettingsPath}");

        Save(defaults, userSettingsPath);
        console.PrintDefault($"Created '{Path.GetFileName(userSettingsPath)}' from appsettings.json defaults.");
        logger.Info($"Created '{userSettingsPath}' from appsettings.json defaults.");
        return defaults;
    }

    public static void Save(Config config, string userSettingsPath)
    {
        File.WriteAllText(userSettingsPath, JsonSerializer.Serialize(config, WriteOptions));
    }
}