using System.Text;

namespace ApolloProfileManager;

/// <summary>
/// Mimics Python's ConfigParser(interpolation=None): no interpolation, case-insensitive keys
/// (stored lowercase), section-based defaults, UTF-8 read/write.
/// </summary>
public class IniConfig
{
    // section -> key -> value  (all keys lowercased like Python ConfigParser)
    private readonly Dictionary<string, Dictionary<string, string>> _data = new(StringComparer.OrdinalIgnoreCase);

    public bool HasSection(string section) => _data.ContainsKey(section);

    public Dictionary<string, string> this[string section]
    {
        get
        {
            if (!_data.TryGetValue(section, out var d))
            {
                d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _data[section] = d;
            }
            return d;
        }
    }

    public IEnumerable<string> Sections => _data.Keys;

    public string? Get(string section, string key) =>
        _data.TryGetValue(section, out var d) && d.TryGetValue(key.ToLowerInvariant(), out var v) ? v : null;

    public void Set(string section, string key, string value)
    {
        if (!_data.ContainsKey(section))
            _data[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _data[section][key.ToLowerInvariant()] = value;
    }

    public void SetDefault(string section, string key, string value)
    {
        if (!_data.ContainsKey(section))
            _data[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var lk = key.ToLowerInvariant();
        if (!_data[section].ContainsKey(lk))
            _data[section][lk] = value;
    }

    public void ClearSection(string section)
    {
        if (_data.ContainsKey(section))
            _data[section].Clear();
    }
}

public static class IniHelper
{
    /// <summary>
    /// Loads an INI file applying section+key defaults, matching Python LoadConfig behavior.
    /// </summary>
    public static IniConfig LoadConfig(string iniPath, Dictionary<string, Dictionary<string, string>>? defaults = null)
    {
        var cfg = new IniConfig();

        if (File.Exists(iniPath))
        {
            string? currentSection = null;
            foreach (var raw in File.ReadAllLines(iniPath, Encoding.UTF8))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#'))
                    continue;
                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    currentSection = line[1..^1].Trim();
                    continue;
                }
                if (currentSection == null) continue;
                var eq = line.IndexOf('=');
                if (eq < 0) continue;
                var k = line[..eq].Trim().ToLowerInvariant();
                var v = line[(eq + 1)..].Trim();
                cfg[currentSection][k] = v;
            }
        }

        // Apply defaults (SetDefault — don't overwrite existing)
        if (defaults != null)
        {
            foreach (var (section, entries) in defaults)
            {
                if (!cfg.HasSection(section))
                    _ = cfg[section]; // ensure section exists
                foreach (var (k, v) in entries)
                    cfg.SetDefault(section, k, v);
            }
        }

        return cfg;
    }

    public static void SaveConfig(IniConfig cfg, string iniPath)
    {
        var dir = Path.GetDirectoryName(iniPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        using var sw = new StreamWriter(iniPath, false, Encoding.UTF8);
        foreach (var section in cfg.Sections)
        {
            sw.WriteLine($"[{section}]");
            foreach (var (k, v) in cfg[section])
                sw.WriteLine($"{k} = {v}");
            sw.WriteLine();
        }
    }

    public static IniConfig LoadMeta(string iniPath) =>
        LoadConfig(iniPath, new() { ["meta"] = new() });

    // ── Overloads accepting Path-like strings ──────────────────────────────────

    public static IniConfig LoadConfig(string iniPath) => LoadConfig(iniPath, null);
}
