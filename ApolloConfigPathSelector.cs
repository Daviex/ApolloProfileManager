namespace ApolloProfileManager;

/// <summary>
/// Handles reading and persisting the Apollo config path from config.ini.
/// Mirrors Python's get_apollo_config_path() function.
/// </summary>
public static class ApolloConfigPathSelector
{
    private static readonly string ConfigIni =
        Path.Combine(PathHelper.GetAppDataDir(), "config.ini");

    public static string ConfigIniPath => ConfigIni;

    /// <summary>
    /// Returns the saved Apollo config path, or prompts the user to select it.
    /// Exits the application if no path is provided.
    /// </summary>
    public static string GetApolloConfigPath()
    {
        IniConfig cfg;
        try
        {
            cfg = IniHelper.LoadConfig(ConfigIni);
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show(
                "Administrator privileges required to read config.ini. Please rerun elevated.",
                "Permission Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
            return string.Empty;
        }

        var saved = cfg.Get("apollo", "apollo_config_path");
        if (!string.IsNullOrEmpty(saved))
            return ExpandHome(saved);

        MessageBox.Show(
            "Apollo config file path not set. Please select the Apollo config file (sunshine.conf).",
            "Apollo Config", MessageBoxButtons.OK, MessageBoxIcon.Information);

        using var dlg = new OpenFileDialog
        {
            Title = "Select Apollo Config File",
            Filter = "Config files (*.conf)|*.conf|All Files (*.*)|*.*",
        };

        if (dlg.ShowDialog() != DialogResult.OK)
        {
            MessageBox.Show("Apollo config path required.", "Apollo Config",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Environment.Exit(1);
            return string.Empty;
        }

        var apolloPath = dlg.FileName;
        if (!ApolloConfigHelper.TryInjectPrepCommands(apolloPath))
        {
            Environment.Exit(1);
            return string.Empty;
        }

        cfg.Set("apollo", "apollo_config_path", apolloPath);
        IniHelper.SaveConfig(cfg, ConfigIni);
        return apolloPath;
    }

    private static string ExpandHome(string path) =>
        path.StartsWith("~")
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                           path[1..].TrimStart('/', '\\'))
            : path;
}
