namespace ApolloProfileManager;

/// <summary>
/// Core save/restore engine. Mirrors Python's do_action() function exactly.
/// </summary>
public static class ProfileEngine
{
    private const string BackupPrefix = "__backup_";
    private const string ClientMetaIni = "client.ini";

    private static readonly Dictionary<string, string> EnvKeys = new()
    {
        ["APP_UUID"]     = "APOLLO_APP_UUID",
        ["CLIENT_UUID"]  = "APOLLO_CLIENT_UUID",
        ["APP_NAME"]     = "APOLLO_APP_NAME",
        ["CLIENT_NAME"]  = "APOLLO_CLIENT_NAME",
        ["PROFILE_DIR"]  = "APOLLO_PROFILE_DIR",
    };

    public static void DoAction(string appDir, string clientDir,
        List<(string PathStr, string HashVal)> pathsWithHashes, string action)
    {
        var appProfile = Path.Combine(appDir, "profile.ini");
        if (!File.Exists(appProfile))
        {
            Console.WriteLine($"App profile not found: {appProfile}, not doing anything.");
            Environment.Exit(0);
        }

        var clientUuid  = Environment.GetEnvironmentVariable(EnvKeys["CLIENT_UUID"]);
        var clientName  = Environment.GetEnvironmentVariable(EnvKeys["CLIENT_NAME"]);

        var backupBase        = Path.Combine(appDir, BackupPrefix + clientUuid);
        var clientProfileBase = clientDir;

        Directory.CreateDirectory(clientProfileBase);

        if (action == "restore")
        {
            // Step 1: Backup current real items (only once)
            if (!Directory.Exists(backupBase))
            {
                Directory.CreateDirectory(backupBase);
                foreach (var (pStr, hash) in pathsWithHashes)
                {
                    var realPath = ExpandUser(pStr);
                    if (!File.Exists(realPath) && !Directory.Exists(realPath))
                    {
                        Console.WriteLine($"[warn] Path not found during backup: {realPath}");
                        continue;
                    }
                    if (Directory.Exists(realPath))
                    {
                        var container = Path.Combine(backupBase, hash);
                        FileSystemHelper.RemoveItem(container);
                        FileSystemHelper.CopyItem(realPath, container);
                    }
                    else
                    {
                        var ext = Path.GetExtension(pStr);
                        var backupFile = Path.Combine(backupBase, hash + ext);
                        FileSystemHelper.RemoveItem(backupFile);
                        FileSystemHelper.CopyItem(realPath, backupFile);
                    }
                }
            }

            // Step 2: Restore client profile items to real paths
            foreach (var (pStr, hash) in pathsWithHashes)
            {
                var realPath = ExpandUser(pStr);
                var ext      = Path.GetExtension(pStr);

                var profileDir  = Path.Combine(clientProfileBase, hash);
                var profileFile = Path.Combine(clientProfileBase, hash + ext);

                if (Directory.Exists(profileDir))
                {
                    FileSystemHelper.RemoveItem(realPath);
                    Directory.CreateDirectory(realPath);
                    FileSystemHelper.CopyItem(profileDir, realPath);
                }
                else if (File.Exists(profileFile))
                {
                    FileSystemHelper.RemoveItem(realPath);
                    var parent = Path.GetDirectoryName(realPath);
                    if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);
                    FileSystemHelper.CopyItem(profileFile, realPath);
                }
            }

            var now = DateTime.Now.ToString("s"); // ISO 8601 seconds precision
            var mg  = IniHelper.LoadConfig(appProfile, new() { ["meta"] = new() });
            mg["meta"]["last_run_time"]   = now;
            mg["meta"]["last_run_client"] = Path.GetFileName(clientDir);
            IniHelper.SaveConfig(mg, appProfile);

            var cmPath = Path.Combine(clientProfileBase, ClientMetaIni);
            var cm     = IniHelper.LoadConfig(cmPath, new() { ["meta"] = new() });
            if (!string.IsNullOrEmpty(clientName)) cm["meta"]["client_name"] = clientName;
            cm["meta"]["last_run_time"] = now;
            IniHelper.SaveConfig(cm, cmPath);
        }
        else if (action == "save")
        {
            foreach (var (pStr, hash) in pathsWithHashes)
            {
                var realPath = ExpandUser(pStr);
                var ext      = Path.GetExtension(pStr);

                var profileDirContainer = Path.Combine(clientProfileBase, hash);
                var profileFile         = Path.Combine(clientProfileBase, hash + ext);

                if (File.Exists(realPath) || Directory.Exists(realPath))
                {
                    if (Directory.Exists(realPath))
                    {
                        if (File.Exists(profileFile)) FileSystemHelper.RemoveItem(profileFile);
                        if (Directory.Exists(profileDirContainer)) FileSystemHelper.RemoveItem(profileDirContainer);
                        FileSystemHelper.CopyItem(realPath, profileDirContainer);
                    }
                    else
                    {
                        if (Directory.Exists(profileDirContainer)) FileSystemHelper.RemoveItem(profileDirContainer);
                        FileSystemHelper.CopyItem(realPath, profileFile);
                    }
                }
                else
                {
                    Console.WriteLine($"[warn] Real path missing during save: {realPath}. Corresponding profile item will not be updated.");
                }

                // Restore from backup to real path
                var backupDir  = Path.Combine(backupBase, hash);
                var backupFile = Path.Combine(backupBase, hash + ext);

                if (Directory.Exists(backupDir))
                {
                    FileSystemHelper.RemoveItem(realPath);
                    Directory.CreateDirectory(realPath);
                    FileSystemHelper.CopyItem(backupDir, realPath);
                }
                else if (File.Exists(backupFile))
                {
                    FileSystemHelper.RemoveItem(realPath);
                    var parent = Path.GetDirectoryName(realPath);
                    if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);
                    FileSystemHelper.CopyItem(backupFile, realPath);
                }
            }

            // Remove entire backup dir for this client
            if (Directory.Exists(backupBase))
                FileSystemHelper.RemoveItem(backupBase);

            var now = DateTime.Now.ToString("s");
            var mg  = IniHelper.LoadConfig(appProfile, new() { ["meta"] = new() });
            mg["meta"]["last_save_time"]   = now;
            mg["meta"]["last_save_client"] = Path.GetFileName(clientDir);
            IniHelper.SaveConfig(mg, appProfile);

            var cmPath = Path.Combine(clientProfileBase, ClientMetaIni);
            var cm     = IniHelper.LoadConfig(cmPath, new() { ["meta"] = new() });
            if (!string.IsNullOrEmpty(clientName)) cm["meta"]["client_name"] = clientName;
            cm["meta"]["last_save_time"] = now;
            IniHelper.SaveConfig(cm, cmPath);
        }

        Console.WriteLine($"{char.ToUpper(action[0]) + action[1..]} finished.");
    }

    /// <summary>Expands leading ~ to home directory (equivalent to Python's Path.expanduser).</summary>
    public static string ExpandUser(string path) =>
        path.StartsWith("~")
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path[1..].TrimStart('/', '\\'))
            : path;
}
