using System.Reflection;
using System.Text.Json;

namespace EasySpire;

public static class SettingsManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static EasySpireSettings _current = EasySpireSettings.Defaults;
    private static string? _configPath;
    private static DateTime _lastWriteTime;

    public static EasySpireSettings Current
    {
        get
        {
            TryReload();
            return _current;
        }
    }

    public static void Initialize()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (string.IsNullOrEmpty(assemblyDir))
            return;

        _configPath = Path.Combine(assemblyDir, "EasySpire.config.json");

        if (!File.Exists(_configPath))
        {
            _current = EasySpireSettings.Defaults;
            Save(_current);
            return;
        }

        Load();
    }

    private static void Load()
    {
        if (_configPath == null || !File.Exists(_configPath))
            return;

        try
        {
            var json = File.ReadAllText(_configPath);
            _current = JsonSerializer.Deserialize<EasySpireSettings>(json, JsonOptions)
                       ?? EasySpireSettings.Defaults;
            _lastWriteTime = File.GetLastWriteTimeUtc(_configPath);
        }
        catch
        {
            _current = EasySpireSettings.Defaults;
        }
    }

    private static void TryReload()
    {
        if (_configPath == null || !File.Exists(_configPath))
            return;

        var currentWriteTime = File.GetLastWriteTimeUtc(_configPath);
        if (currentWriteTime > _lastWriteTime)
        {
            Load();
        }
    }

    public static void Save(EasySpireSettings settings)
    {
        if (_configPath == null)
            return;

        try
        {
            _current = settings;
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_configPath, json);
            _lastWriteTime = File.GetLastWriteTimeUtc(_configPath);
        }
        catch
        {
            // Silently fail - don't crash the game
        }
    }

    public static void UpdateFeature(string featureName, bool? enabled = null, double? value = null)
    {
        var settings = Current;
        var prop = typeof(EasySpireSettings).GetProperty(featureName);
        if (prop == null) return;

        var toggle = (FeatureToggle?)prop.GetValue(settings);
        if (toggle == null) return;

        var updated = toggle with
        {
            Enabled = enabled ?? toggle.Enabled,
            Value = value ?? toggle.Value
        };

        // Use reflection to create a new settings with the updated property
        var constructor = typeof(EasySpireSettings).GetConstructor(Type.EmptyTypes)!;
        var newSettings = (EasySpireSettings)constructor.Invoke(null);

        // Copy all properties, replacing the one we changed
        foreach (var p in typeof(EasySpireSettings).GetProperties()
                     .Where(p => p.PropertyType == typeof(FeatureToggle)))
        {
            if (p.Name == featureName)
                continue; // Will be set below
        }

        // Build new settings with record 'with' via reflection workaround
        newSettings = settings with { };
        prop.SetValue(newSettings, updated); // Records with init setters can be set via reflection

        Save(newSettings);
    }
}
