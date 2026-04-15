using System.Text.RegularExpressions;

namespace ApolloProfileManager;

public partial class ClientManagerDialog : Form
{
    private readonly string _appPath;
    private List<string> _clientDirs = new();

    private static readonly Regex ClientDirRegex =
        new(@"^[0-9A-Fa-f\-]{36}$", RegexOptions.Compiled);

    public ClientManagerDialog(string appPath, string appName)
    {
        _appPath = appPath;

        InitializeComponent();

        Text = $"Saves for {appName} ({{{Path.GetFileName(appPath)}}})";
        RefreshClients();
    }

    // ── Event handlers (wired up in Designer) ────────────────────────────────

    private void LstClients_SelectedIndexChanged(object? sender, EventArgs e) => OnSelectClient();
    private void BtnOpenDir_Click(object? sender, EventArgs e) => OpenDir();
    private void BtnDeleteClient_Click(object? sender, EventArgs e) => DeleteClient();

    // ── Refresh ───────────────────────────────────────────────────────────────

    private void RefreshClients()
    {
        _clientDirs = Directory.Exists(_appPath)
            ? Directory.GetDirectories(_appPath)
                .Select(Path.GetFileName)
                .Where(n => n != null && ClientDirRegex.IsMatch(n!) && !n!.StartsWith("__backup_"))
                .Select(n => Path.Combine(_appPath, n!))
                .OrderBy(d =>
                {
                    var m = IniHelper.LoadMeta(Path.Combine(d, "client.ini"));
                    return (m.Get("meta", "client_name") ?? Path.GetFileName(d)).ToLowerInvariant();
                })
                .ToList()
            : new List<string>();

        lstClients.BeginUpdate();
        lstClients.Items.Clear();
        foreach (var d in _clientDirs)
        {
            var meta = IniHelper.LoadMeta(Path.Combine(d, "client.ini"));
            lstClients.Items.Add(meta.Get("meta", "client_name") ?? Path.GetFileName(d));
        }
        lstClients.EndUpdate();

        OnSelectClient();
    }

    // ── Selection ─────────────────────────────────────────────────────────────

    private void OnSelectClient()
    {
        var sel = lstClients.SelectedIndex;
        btnOpenDir.Enabled = btnDeleteClient.Enabled = sel >= 0;

        if (sel < 0)
        {
            lblClientName.Text = "Name: \u2014";
            lblClientUuid.Text = "UUID: {\u2014}";
            lblClientRun.Text  = "Last run: \u2014";
            lblClientSave.Text = "Last save: \u2014";
            return;
        }

        var d    = _clientDirs[sel];
        var meta = IniHelper.LoadMeta(Path.Combine(d, "client.ini"))["meta"];
        lblClientUuid.Text = $"UUID: {{{Path.GetFileName(d)}}}";
        lblClientName.Text = $"Name: {meta.GetValueOrDefault("client_name", "N/A")}";
        lblClientRun.Text  = $"Last run: {meta.GetValueOrDefault("last_run_time",  "\u2014")}";
        lblClientSave.Text = $"Last save: {meta.GetValueOrDefault("last_save_time", "\u2014")}";
    }

    // ── Button actions ────────────────────────────────────────────────────────

    private void OpenDir()
    {
        var sel = lstClients.SelectedIndex;
        if (sel < 0) return;
        System.Diagnostics.Process.Start("explorer.exe", _clientDirs[sel]);
    }

    private void DeleteClient()
    {
        var sel = lstClients.SelectedIndex;
        if (sel < 0) return;
        var d = _clientDirs[sel];
        if (MessageBox.Show(this, $"Delete client {Path.GetFileName(d)}?", "Delete client?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            FileSystemHelper.RemoveItem(d);
            RefreshClients();
        }
    }
}
