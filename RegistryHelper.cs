using System.Diagnostics;
using Microsoft.Win32;

namespace ApolloProfileManager;

public static class RegistryHelper
{
    private static readonly Dictionary<string, string> HiveAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["HKCU"] = "HKEY_CURRENT_USER",
        ["HKLM"] = "HKEY_LOCAL_MACHINE",
        ["HKCR"] = "HKEY_CLASSES_ROOT",
        ["HKU"] = "HKEY_USERS",
        ["HKCC"] = "HKEY_CURRENT_CONFIG",
    };

    public static string NormalizeKeyPath(string path)
    {
        var trimmed = path.Trim().Trim('"').Replace('/', '\\');
        if (trimmed.StartsWith("Computer\\", StringComparison.OrdinalIgnoreCase))
            trimmed = trimmed["Computer\\".Length..];
        if (trimmed.EndsWith(":\\", StringComparison.Ordinal))
            trimmed = trimmed[..^2];
        else if (trimmed.EndsWith(":", StringComparison.Ordinal))
            trimmed = trimmed[..^1];

        while (trimmed.Contains("\\\\", StringComparison.Ordinal))
            trimmed = trimmed.Replace("\\\\", "\\");
        trimmed = trimmed.TrimEnd('\\');

        var slash = trimmed.IndexOf('\\');
        var hive = slash >= 0 ? trimmed[..slash] : trimmed;
        if (hive.EndsWith(":", StringComparison.Ordinal))
        {
            hive = hive[..^1];
            trimmed = slash >= 0 ? hive + trimmed[slash..] : hive;
            slash = trimmed.IndexOf('\\');
        }
        if (HiveAliases.TryGetValue(hive, out var fullHive))
            trimmed = slash >= 0 ? fullHive + trimmed[slash..] : fullHive;

        if (!TrySplitKeyPath(trimmed, out _, out var subKey) || string.IsNullOrWhiteSpace(subKey))
            throw new ArgumentException("Enter a registry key below a root hive, for example HKCU\\Software\\Vendor\\Game.");

        return trimmed;
    }

    public static bool KeyExists(string keyPath)
    {
        var normalized = NormalizeKeyPath(keyPath);
        if (!TrySplitKeyPath(normalized, out var hive, out var subKey))
            return false;

        using var key = hive.OpenSubKey(subKey, writable: false);
        return key != null;
    }

    public static bool ExportKey(string keyPath, string destinationRegFile)
    {
        var normalized = NormalizeKeyPath(keyPath);
        if (!KeyExists(normalized))
            return false;

        var parent = Path.GetDirectoryName(destinationRegFile);
        if (!string.IsNullOrEmpty(parent))
            Directory.CreateDirectory(parent);

        RunReg("export", $"\"{normalized}\" \"{destinationRegFile}\" /y", throwOnFailure: true);
        return true;
    }

    public static void ImportKeyReplacingCurrent(string keyPath, string sourceRegFile)
    {
        if (!File.Exists(sourceRegFile))
            return;

        DeleteKey(keyPath);
        RunReg("import", $"\"{sourceRegFile}\"", throwOnFailure: true);
    }

    public static void DeleteKey(string keyPath)
    {
        var normalized = NormalizeKeyPath(keyPath);
        if (!KeyExists(normalized))
            return;

        RunReg("delete", $"\"{normalized}\" /f", throwOnFailure: true);
    }

    private static bool TrySplitKeyPath(string keyPath, out RegistryKey hive, out string subKey)
    {
        var slash = keyPath.IndexOf('\\');
        var hiveName = slash >= 0 ? keyPath[..slash] : keyPath;
        subKey = slash >= 0 ? keyPath[(slash + 1)..] : string.Empty;

        hive = hiveName.ToUpperInvariant() switch
        {
            "HKEY_CURRENT_USER" => Registry.CurrentUser,
            "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
            "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
            "HKEY_USERS" => Registry.Users,
            "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
            _ => null!,
        };

        return hive != null;
    }

    private static void RunReg(string verb, string arguments, bool throwOnFailure)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "reg.exe",
            Arguments = $"{verb} {arguments}",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        });

        if (process == null)
            throw new InvalidOperationException("Could not start reg.exe.");

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        var output = outputTask.GetAwaiter().GetResult();
        var error = errorTask.GetAwaiter().GetResult();

        if (process.ExitCode == 0 || !throwOnFailure)
            return;

        throw new InvalidOperationException($"reg.exe {verb} failed for {arguments}: {error}{output}");
    }
}
