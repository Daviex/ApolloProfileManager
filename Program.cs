namespace ApolloProfileManager;

static class Program
{
    private static readonly Dictionary<string, string> EnvKeys = new()
    {
        ["APP_UUID"]    = "APOLLO_APP_UUID",
        ["CLIENT_UUID"] = "APOLLO_CLIENT_UUID",
    };

    [STAThread]
    static void Main(string[] rawArgs)
    {
        ApplicationConfiguration.Initialize();

        try
        {
            RunMain(rawArgs);
        }
        catch (Exception ex)
        {
            var tb = ex.ToString();
            Console.Error.WriteLine(tb);
            var msg = $"An unexpected error occurred:\n\n{ex.GetType().Name}: {ex.Message}\n\n{tb}";
            ErrorDialogHelper.SpawnErrorDialogAndExitZero(msg);
        }
    }

    private static void RunMain(string[] args)
    {
        // ── Hidden: --show-error-dialog-test <text> ───────────────────────────
        var testIdx = Array.IndexOf(args, "--show-error-dialog-test");
        if (testIdx >= 0 && testIdx + 1 < args.Length)
        {
            ErrorDialogHelper.SpawnErrorDialogAndExitZero($"This is a test error message:\n\n{args[testIdx + 1]}");
            return;
        }

        // ── Hidden: --show-error-dialog <base64> ──────────────────────────────
        var dialogIdx = Array.IndexOf(args, "--show-error-dialog");
        if (dialogIdx >= 0 && dialogIdx + 1 < args.Length)
        {
            ApplicationConfiguration.Initialize();
            ErrorDialogHelper.ShowErrorDialog(args[dialogIdx + 1]);
            return;
        }

        // ── Hidden: --inject-config <path> ────────────────────────────────────
        var injectIdx = Array.IndexOf(args, "--inject-config");
        if (injectIdx >= 0 && injectIdx + 1 < args.Length)
        {
            var apolloPath = args[injectIdx + 1].Trim('"');
            if (!File.Exists(apolloPath))
            {
                Console.Error.WriteLine($"Apollo config file not found: {apolloPath}");
                Environment.Exit(1);
            }
            try
            {
                ApolloConfigHelper.InjectPrepCommands(apolloPath);
                ApplicationConfiguration.Initialize();
                MessageBox.Show(
                    "Prep commands injected successfully. Please restart Apollo to take effect.",
                    "Apollo config", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                ApplicationConfiguration.Initialize();
                MessageBox.Show(
                    $"Failed to inject prep commands: {ex.Message}",
                    "Failed to inject prep commands", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            return;
        }

        // ── CLI commands: restore | save ──────────────────────────────────────
        var command = args.Length > 0 ? args[0].ToLowerInvariant() : null;
        if (command == "restore" || command == "save")
        {
            var aId     = Environment.GetEnvironmentVariable(EnvKeys["APP_UUID"]);
            var cId     = Environment.GetEnvironmentVariable(EnvKeys["CLIENT_UUID"]);

            if (string.IsNullOrEmpty(aId) || string.IsNullOrEmpty(cId))
            {
                Console.Error.WriteLine("APOLLO_APP_UUID & APOLLO_CLIENT_UUID required.");
                Environment.Exit(1);
            }

            if (!Guid.TryParse(aId, out _) || !Guid.TryParse(cId, out _))
            {
                Console.Error.WriteLine("Invalid UUID format.");
                Environment.Exit(1);
            }

            var cfg    = IniHelper.LoadConfig(ApolloConfigPathSelector.ConfigIniPath);
            var rd     = PathHelper.GetProfilesDir(cfg);
            var appDir = Path.Combine(rd, aId!);
            var client = Path.Combine(appDir, cId!);

            ProfileEngine.DoAction(appDir, client, PathHelper.GetAppPaths(appDir), command);
            return;
        }

        // ── GUI mode ──────────────────────────────────────────────────────────
        // Warn if elevated (drag-and-drop won't work when elevated)
        if (AdminHelper.IsElevated())
        {
            var proceed = MessageBox.Show(
                "This process is running with elevated privileges. " +
                "Drag and drop files to the file manager window may not work. Do you want to continue?",
                "Elevated Process", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (proceed != DialogResult.Yes)
            {
                Environment.Exit(1);
                return;
            }
        }

        var apolloCfg   = ApolloConfigPathSelector.GetApolloConfigPath();
        var appCfg      = IniHelper.LoadConfig(ApolloConfigPathSelector.ConfigIniPath);
        var profilesDir = PathHelper.GetProfilesDir(appCfg);

        var preselect = Environment.GetEnvironmentVariable(EnvKeys["APP_UUID"]);
        Application.Run(new ProfileManagerGUI(apolloCfg, profilesDir, preselect));
    }
}
