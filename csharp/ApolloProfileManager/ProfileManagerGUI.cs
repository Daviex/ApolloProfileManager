using System.Text.Json;

namespace ApolloProfileManager;

public partial class ProfileManagerGUI : Form
{
    private string _apolloPath;
    private readonly string _rootDir;
    private readonly string? _preselect;
    private List<(string Uid, string Name)> _apps = new();

    public ProfileManagerGUI(string apolloPath, string rootDir, string? preselect = null)
    {
        _apolloPath = apolloPath;
        _rootDir    = rootDir;
        _preselect  = preselect;

        InitializeComponent();
        RefreshGames();

        if (_preselect != null)
        {
            for (int i = 0; i < _apps.Count; i++)
            {
                if (_apps[i].Uid == _preselect)
                {
                    lstGames.SelectedIndex = i;
                    break;
                }
            }
        }
    }

    // ── Event handlers (wired up in Designer) ────────────────────────────────

    private void LstGames_SelectedIndexChanged(object? sender, EventArgs e) => OnSelectGame();
    private void LstGames_DoubleClick(object? sender, EventArgs e) => ManageClientSaves();
    private void BtnEdit_Click(object? sender, EventArgs e) => EditPaths();
    private void BtnManage_Click(object? sender, EventArgs e) => ManageClientSaves();
    private void BtnOpen_Click(object? sender, EventArgs e) => OpenAppDir();
    private void BtnDelete_Click(object? sender, EventArgs e) => DeleteApp();
    private void BtnInject_Click(object? sender, EventArgs e) => InjectPrepCommands();
    private void BtnConfig_Click(object? sender, EventArgs e) => ChooseConfig();

    // ── Game list ─────────────────────────────────────────────────────────────

    private void RefreshGames()
    {
        _apps = LoadApps();
        lstGames.BeginUpdate();
        lstGames.Items.Clear();
        foreach (var (_, name) in _apps)
            lstGames.Items.Add(name);
        lstGames.EndUpdate();

        if (lstGames.SelectedIndex >= _apps.Count)
            lstGames.ClearSelected();
        OnSelectGame();
    }

    private List<(string Uid, string Name)> LoadApps()
    {
        var loaded = new List<(string, string)>();

        var appsJsonFilename = GetAppsJsonFilename();
        var appsJsonPath = Path.Combine(Path.GetDirectoryName(_apolloPath)!, appsJsonFilename);

        if (File.Exists(appsJsonPath))
        {
            try
            {
                using var stream = File.OpenRead(appsJsonPath);
                var doc = JsonDocument.Parse(stream);
                if (doc.RootElement.TryGetProperty("apps", out var arr))
                {
                    foreach (var app in arr.EnumerateArray())
                    {
                        if (app.TryGetProperty("uuid", out var uuidEl) &&
                            !string.IsNullOrEmpty(uuidEl.GetString()))
                        {
                            var uid  = uuidEl.GetString()!;
                            var name = app.TryGetProperty("name", out var n) ? n.GetString() ?? uid : uid;
                            loaded.Add((uid, name));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to load or parse {appsJsonPath}:\n{ex.Message}",
                    "Error loading apps.json", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            MessageBox.Show(this, $"Expected apps file not found at: {appsJsonPath}",
                "apps.json not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // Append extra UUID dirs in profiles_dir not listed in apps.json
        if (Directory.Exists(_rootDir))
        {
            var existing = new HashSet<string>(loaded.Select(a => a.Item1), StringComparer.OrdinalIgnoreCase);
            foreach (var dir in Directory.GetDirectories(_rootDir))
            {
                var dirName = Path.GetFileName(dir);
                if (!Guid.TryParse(dirName, out _) || existing.Contains(dirName)) continue;
                var cfg  = IniHelper.LoadConfig(Path.Combine(dir, "profile.ini"), new() { ["meta"] = new() });
                var name = cfg.Get("meta", "app_name") ?? dirName;
                loaded.Add((dirName, $"{name} (deleted)"));
            }
        }

        return loaded;
    }

    private string GetAppsJsonFilename()
    {
        if (string.IsNullOrEmpty(_apolloPath) || !File.Exists(_apolloPath))
        {
            MessageBox.Show(this, "Apollo configuration path is not set or file does not exist.",
                "Apollo Config Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return "apps.json";
        }
        try
        {
            return ApolloConfigHelper.GetAppsJsonFilename(_apolloPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not read app list file name from {_apolloPath}:\n{ex.Message}",
                "Error reading Apollo Config", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return "apps.json";
        }
    }

    // ── Selection ─────────────────────────────────────────────────────────────

    private void OnSelectGame()
    {
        var sel = lstGames.SelectedIndex;
        if (sel < 0)
        {
            SetSelectionButtons(false);
            lblName.Text     = "Name: \u2014";
            lblUuid.Text     = "UUID: \u2014";
            lblLastRun.Text  = "Last run: \u2014 (\u2014)";
            lblLastSave.Text = "Last save: \u2014 (\u2014)";
            return;
        }

        var (uid, name) = _apps[sel];
        lblName.Text = $"Name: {name}";
        lblUuid.Text = $"UUID: {uid}";

        var ini = Path.Combine(_rootDir, uid, "profile.ini");
        if (File.Exists(ini))
        {
            var cfg = IniHelper.LoadConfig(ini, new() { ["meta"] = new() });
            var lr  = cfg.Get("meta", "last_run_time")  ?? "\u2014";
            var ls  = cfg.Get("meta", "last_save_time") ?? "\u2014";
            var lrc = ResolveClientName(uid, cfg.Get("meta", "last_run_client"))  ?? "\u2014";
            var lsc = ResolveClientName(uid, cfg.Get("meta", "last_save_client")) ?? "\u2014";
            lblLastRun.Text  = $"Last run: {lr} ({lrc})";
            lblLastSave.Text = $"Last save: {ls} ({lsc})";
            SetSelectionButtons(true);
        }
        else
        {
            lblLastRun.Text  = "Last run: \u2014 (\u2014)";
            lblLastSave.Text = "Last save: \u2014 (\u2014)";
            btnManage.Enabled = btnOpen.Enabled = btnDelete.Enabled = false;
        }
        btnEdit.Enabled = true;
    }

    private string? ResolveClientName(string uid, string? clientUuid)
    {
        if (string.IsNullOrEmpty(clientUuid)) return null;
        var clientIni = Path.Combine(_rootDir, uid, clientUuid, "client.ini");
        if (!File.Exists(clientIni)) return clientUuid;
        return IniHelper.LoadMeta(clientIni).Get("meta", "client_name") ?? clientUuid;
    }

    private void SetSelectionButtons(bool enabled) =>
        btnEdit.Enabled = btnManage.Enabled = btnOpen.Enabled = btnDelete.Enabled = enabled;

    // ── Profile helpers ───────────────────────────────────────────────────────

    private bool EnsureProfileIni(string appDir, string appName)
    {
        var ini = Path.Combine(appDir, "profile.ini");
        if (File.Exists(ini)) return true;
        if (MessageBox.Show(this, $"Do you want to create a new profile for {appName}?",
                "Profile not found", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return false;
        Directory.CreateDirectory(appDir);
        var cfg = IniHelper.LoadConfig(ini, new() { ["meta"] = new() });
        cfg["meta"]["app_name"] = appName;
        IniHelper.SaveConfig(cfg, ini);
        return true;
    }

    // ── Button actions ────────────────────────────────────────────────────────

    private void OpenAppDir()
    {
        var sel = lstGames.SelectedIndex;
        if (sel < 0) return;
        var (uid, name) = _apps[sel];
        var appDir = Path.Combine(_rootDir, uid);
        if (!EnsureProfileIni(appDir, name)) return;
        System.Diagnostics.Process.Start("explorer.exe", appDir);
    }

    private void DeleteApp()
    {
        var sel = lstGames.SelectedIndex;
        if (sel < 0) return;
        var (uid, name) = _apps[sel];
        var appDir = Path.Combine(_rootDir, uid);
        if (!Directory.Exists(appDir))
        {
            MessageBox.Show(this, "App profile does not exist.", "Nothing to delete",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (MessageBox.Show(this, $"Delete app {name}?", "Delete app?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            FileSystemHelper.RemoveItem(appDir);
            RefreshGames();
        }
    }

    private void ManageClientSaves()
    {
        var sel = lstGames.SelectedIndex;
        if (sel < 0) return;
        var (uid, name) = _apps[sel];
        var appDir = Path.Combine(_rootDir, uid);
        if (!EnsureProfileIni(appDir, name)) return;
        using var dlg = new ClientManagerDialog(appDir, name);
        dlg.ShowDialog(this);
    }

    private void EditPaths()
    {
        var sel = lstGames.SelectedIndex;
        if (sel < 0) return;
        var (uid, name) = _apps[sel];
        var appDir = Path.Combine(_rootDir, uid);
        if (!EnsureProfileIni(appDir, name)) return;
        using var dlg = new PathEditorDialog(appDir, name);
        dlg.ShowDialog(this);
    }

    private void InjectPrepCommands()
    {
        ApolloConfigHelper.TryInjectPrepCommands(_apolloPath, this);
        RefreshGames();
    }

    private void ChooseConfig()
    {
        using var dlg = new OpenFileDialog
        {
            Title    = "Select Apollo Config File (sunshine.conf)",
            Filter   = "Config files (*.conf)|*.conf|All Files (*.*)|*.*",
            FileName = "sunshine.conf",
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        var newPath = dlg.FileName;
        if (!File.Exists(newPath))
        {
            MessageBox.Show(this, $"The selected path '{newPath}' is not a file.",
                "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (newPath == _apolloPath)
        {
            MessageBox.Show(this, "The selected configuration file is already in use.",
                "No Change", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var old = _apolloPath;
        _apolloPath = newPath;

        var cfg = IniHelper.LoadConfig(ApolloConfigPathSelector.ConfigIniPath);
        cfg.Set("apollo", "apollo_config_path", newPath);
        try
        {
            IniHelper.SaveConfig(cfg, ApolloConfigPathSelector.ConfigIniPath);
            MessageBox.Show(this, $"Apollo config path updated to: {newPath}",
                "Config Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ApolloConfigHelper.TryInjectPrepCommands(_apolloPath, this);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to save config.ini: {ex.Message}",
                "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _apolloPath = old;
        }

        RefreshGames();
    }
}
