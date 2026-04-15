using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApolloProfileManager;

public static class ApolloConfigHelper
{
    private const string SectionHeader = "[root]";

    /// <summary>
    /// Parses sunshine.conf (no section headers) by prepending [root] then treating it as INI.
    /// </summary>
    public static IniConfig ParseApolloConfig(string configPath)
    {
        var text = File.ReadAllText(configPath, Encoding.UTF8);
        // Write to temp string with section header prepended
        var combined = SectionHeader + "\n" + text;
        var cfg = new IniConfig();
        string? currentSection = null;
        foreach (var rawLine in combined.Split('\n'))
        {
            var line = rawLine.Trim();
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
        return cfg;
    }

    /// <summary>
    /// Returns the apps JSON filename from sunshine.conf (file_apps key, default "apps.json").
    /// </summary>
    public static string GetAppsJsonFilename(string apolloConfigPath)
    {
        if (!File.Exists(apolloConfigPath))
            return "apps.json";
        try
        {
            var cfg = ParseApolloConfig(apolloConfigPath);
            return cfg.Get("root", "file_apps") ?? "apps.json";
        }
        catch
        {
            return "apps.json";
        }
    }

    /// <summary>
    /// Injects do/undo prep commands into sunshine.conf global_prep_cmd JSON array.
    /// Matching Python inject_prep_commands().
    /// </summary>
    /// <summary>
    /// Injects do/undo prep commands into sunshine.conf global_prep_cmd JSON array.
    /// Returns true if commands were injected, false if identical commands already exist.
    /// </summary>
    public static bool InjectPrepCommands(string apolloCfgPath)
    {
        var text = File.ReadAllText(apolloCfgPath, Encoding.UTF8);
        var cfg = ParseApolloConfig(apolloCfgPath);
        var root = cfg["root"];

        JsonArray gpList;
        if (root.TryGetValue("global_prep_cmd", out var gpRaw) && !string.IsNullOrWhiteSpace(gpRaw))
        {
            gpList = JsonNode.Parse(gpRaw)?.AsArray() ?? new JsonArray();
        }
        else
        {
            gpList = new JsonArray();
        }

        var exePath = $"\"{(Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule!.FileName)}\"";
        var doCmd   = $"{exePath} restore";
        var undoCmd = $"{exePath} save";

        // Check if an identical entry already exists.
        foreach (var item in gpList)
        {
            var obj = item?.AsObject();
            if (obj == null) continue;
            if (obj["do"]?.GetValue<string>()   == doCmd &&
                obj["undo"]?.GetValue<string>() == undoCmd)
                return false;
        }

        var newEntry = new JsonObject
        {
            ["do"]       = doCmd,
            ["undo"]     = undoCmd,
            ["elevated"] = true,
        };

        // Insert at front
        gpList.Insert(0, newEntry);

        // Rewrite the config file preserving all other keys
        var lines = text.Split('\n');
        bool gpWritten = false;
        var sb = new StringBuilder();
        foreach (var rawLine in lines)
        {
            var trimmed = rawLine.TrimStart();
            var eqIdx = trimmed.IndexOf('=');
            if (eqIdx > 0)
            {
                var k = trimmed[..eqIdx].Trim().ToLowerInvariant();
                if (k == "global_prep_cmd")
                {
                    sb.AppendLine($"global_prep_cmd = {gpList.ToJsonString()}");
                    gpWritten = true;
                    continue;
                }
            }
            sb.AppendLine(rawLine.TrimEnd('\r'));
        }
        if (!gpWritten)
            sb.AppendLine($"global_prep_cmd = {gpList.ToJsonString()}");

        File.WriteAllText(apolloCfgPath, sb.ToString(), Encoding.UTF8);
        return true;
    }

    /// <summary>
    /// Shows confirmation dialog, then injects prep commands.
    /// Handles PermissionError by offering UAC elevation. Returns true on success.
    /// </summary>
    public static bool TryInjectPrepCommands(string apolloCfgPath, IWin32Window? owner = null)
    {
        var confirm = MessageBox.Show(
            owner,
            "Are you sure you want to inject do/undo prep commands into the Apollo config now? " +
            "Please make sure you have already removed the existing prep commands for the profile manager.",
            "Inject Prep Commands",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
            return false;

        try
        {
            bool injected = InjectPrepCommands(apolloCfgPath);
            if (!injected)
            {
                MessageBox.Show(owner,
                    "Prep commands for this executable are already present in the Apollo config. No changes were made.",
                    "Apollo Config", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }
            MessageBox.Show(owner,
                "Prep commands injected successfully. Please restart Apollo to take effect.",
                "Apollo Config", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            var ask = MessageBox.Show(owner,
                "Administrator privileges required to modify Apollo config. Would you like to run with elevated privileges?",
                "Permission Denied", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (ask != DialogResult.Yes)
                return false;

            return RelaunchElevatedForInject(apolloCfgPath, owner);
        }
    }

    private static bool RelaunchElevatedForInject(string apolloCfgPath, IWin32Window? owner)
    {
        var exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule!.FileName;
        var result = NativeMethods.ShellExecute(
            IntPtr.Zero, "runas", exePath,
            $"--inject-config \"{apolloCfgPath}\"",
            null, 1);

        if (result <= 32)
        {
            MessageBox.Show(owner,
                "Failed to run as admin. Please run with elevated privileges manually.",
                "Apollo Config", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        return true;
    }
}

internal static class NativeMethods
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile,
        string? lpParameters, string? lpDirectory, int nShowCmd);
}
