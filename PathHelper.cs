using System.Security.Cryptography;
using System.Text;

namespace ApolloProfileManager;

public static class PathHelper
{
    /// <summary>Returns the directory containing the running executable.</summary>
    public static string GetBaseDir()
    {
        var exe = Environment.ProcessPath ?? AppContext.BaseDirectory;
        return Path.GetDirectoryName(Path.GetFullPath(exe)) ?? AppContext.BaseDirectory;
    }

    /// <summary>
    /// Returns %LOCALAPPDATA%\ApolloProfileManager — the persistent user-data directory
    /// used for profiles and config, independent of where the executable lives.
    /// </summary>
    public static string GetAppDataDir()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "ApolloProfileManager");
    }

    /// <summary>
    /// Converts an absolute path to a relative form by stripping root/drive,
    /// matching Python's make_rel() behavior.
    /// </summary>
    public static string MakeRel(string path)
    {
        var p = Path.GetFullPath(path);
        var root = Path.GetPathRoot(p) ?? string.Empty;
        // strip drive colon like Python's p.drive.rstrip(":")
        var stripped = root.TrimEnd('\\', '/', ':');
        var tail = p[root.Length..]; // portion after root
        return Path.Combine(stripped, tail.TrimStart('\\', '/'));
    }

    /// <summary>Returns SHA1 hex digest of a UTF-8 encoded string (matches Python hashlib.sha1).</summary>
    public static string Sha1Hex(string input)
    {
        var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Reads profile.ini [paths] section and returns list of (pathString, sha1Hash) tuples.
    /// </summary>
    public static List<(string PathStr, string HashVal)> GetAppPaths(string appDir)
    {
        var iniPath = Path.Combine(appDir, "profile.ini");
        var cfg = IniHelper.LoadConfig(iniPath, new() { ["paths"] = new() });
        var result = new List<(string, string)>();
        foreach (var (hashVal, pathStr) in cfg["paths"])
            result.Add((pathStr, hashVal));
        return result;
    }

    /// <summary>
    /// Writes a list of path strings to profile.ini [paths], keyed by SHA1 hash.
    /// </summary>
    public static void SetAppPaths(string appDir, IEnumerable<string> paths)
    {
        var iniPath = Path.Combine(appDir, "profile.ini");
        var cfg = IniHelper.LoadConfig(iniPath, new() { ["paths"] = new() });
        cfg.ClearSection("paths");
        foreach (var p in paths)
            cfg["paths"][Sha1Hex(p)] = p;
        IniHelper.SaveConfig(cfg, iniPath);
    }
}
