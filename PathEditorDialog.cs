namespace ApolloProfileManager;

public partial class PathEditorDialog : Form
{
    private readonly string _appPath;

    public PathEditorDialog(string appPath, string appName)
    {
        _appPath = appPath;

        InitializeComponent();

        Text = $"Edit Tracked Paths for {appName}";
        RefreshList();
    }

    // ── Event handlers (wired up in Designer) ────────────────────────────────

    private void LstPaths_DragEnter(object? sender, DragEventArgs e)
    {
        e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private void LstPaths_DragDrop(object? sender, DragEventArgs e) => HandleDrop(e);
    private void BtnAddDir_Click(object? sender, EventArgs e)  => AddPath(isDir: true);
    private void BtnAddFile_Click(object? sender, EventArgs e) => AddPath(isDir: false);
    private void BtnAddRegistry_Click(object? sender, EventArgs e) => AddRegistryKey();
    private void BtnRemove_Click(object? sender, EventArgs e)  => RemovePath();
    private void BtnClose_Click(object? sender, EventArgs e)   => Close();

    // ── List refresh ──────────────────────────────────────────────────────────

    private void RefreshList()
    {
        lstPaths.BeginUpdate();
        lstPaths.Items.Clear();
        foreach (var (pStr, _) in PathHelper.GetAppPaths(_appPath))
            lstPaths.Items.Add($"File/Folder: {pStr}");
        foreach (var (keyPath, _) in PathHelper.GetAppRegistryKeys(_appPath))
            lstPaths.Items.Add($"Registry: {keyPath}");
        lstPaths.EndUpdate();
    }

    // ── Path conflict validation ──────────────────────────────────────────────

    /// <summary>
    /// Returns true if pToCheck has no parent/child overlap with existing tracked paths.
    /// Mirrors Python's _check_path_conflicts().
    /// </summary>
    private bool CheckPathConflicts(string pToCheck, List<string> currentPaths, string contextPrefix)
    {
        if (!File.Exists(pToCheck) && !Directory.Exists(pToCheck))
        {
            MessageBox.Show(this, $"{contextPrefix}\n'{pToCheck}'\ndoes not exist.",
                "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        var resolved = Path.GetFullPath(pToCheck);

        // Check 1: Is pToCheck a child of an already-tracked directory?
        foreach (var existing in currentPaths)
        {
            if (Directory.Exists(Path.GetFullPath(existing)) &&
                IsSubPath(resolved, Path.GetFullPath(existing)))
            {
                MessageBox.Show(this,
                    $"{contextPrefix}\n'{pToCheck}'\nis already covered by the tracked directory\n'{existing}'.",
                    "Path Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        // Check 2: If pToCheck is a directory, does it contain any existing tracked path?
        if (Directory.Exists(resolved))
        {
            foreach (var existing in currentPaths)
            {
                if (IsSubPath(Path.GetFullPath(existing), resolved))
                {
                    MessageBox.Show(this,
                        $"{contextPrefix} directory\n'{pToCheck}'\ncontains an already tracked path\n'{existing}'.\n" +
                        "Please remove the inner path first or add a more specific directory.",
                        "Path Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
        }

        return true;
    }

    private static bool IsSubPath(string child, string parent)
    {
        var childUri  = new Uri(child.TrimEnd('\\', '/') + Path.DirectorySeparatorChar);
        var parentUri = new Uri(parent.TrimEnd('\\', '/') + Path.DirectorySeparatorChar);
        return parentUri.IsBaseOf(childUri) && !parentUri.Equals(childUri);
    }

    private bool CheckRegistryConflicts(string keyToCheck, List<string> currentKeys)
    {
        var normalized = RegistryHelper.NormalizeKeyPath(keyToCheck);

        foreach (var existing in currentKeys)
        {
            if (string.Equals(normalized, existing, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, $"The registry key\n'{normalized}'\nis already tracked.",
                    "Duplicate Registry Key", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (IsRegistryChild(normalized, existing) || IsRegistryChild(existing, normalized))
            {
                MessageBox.Show(this,
                    $"The registry key\n'{normalized}'\noverlaps with already tracked key\n'{existing}'.",
                    "Registry Key Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        return true;
    }

    private static bool IsRegistryChild(string child, string parent) =>
        child.StartsWith(parent.TrimEnd('\\') + "\\", StringComparison.OrdinalIgnoreCase);

    // ── Drag and drop ─────────────────────────────────────────────────────────

    private void HandleDrop(DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is not string[] dropped) return;

        var current  = PathHelper.GetAppPaths(_appPath).Select(x => x.PathStr).ToList();
        var toAdd    = new List<string>();
        bool hadIssue = false;

        foreach (var p in dropped)
        {
            if (current.Contains(p, StringComparer.OrdinalIgnoreCase)) { hadIssue = true; continue; }
            if (!CheckPathConflicts(p, current, "Dropped path"))       { hadIssue = true; continue; }
            if (!toAdd.Contains(p, StringComparer.OrdinalIgnoreCase))  toAdd.Add(p);
        }

        if (toAdd.Count > 0)
        {
            PathHelper.SetAppPaths(_appPath, current.Concat(toAdd));
            RefreshList();
            if (hadIssue)
                MessageBox.Show(this,
                    "Some dropped items were added. Others were skipped due to conflicts, non-existence, or being duplicates.",
                    "Drag & Drop Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else if (hadIssue)
        {
            MessageBox.Show(this,
                "No items were added from the drop operation due to conflicts, non-existence, or being duplicates.",
                "Drag & Drop Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // ── Button actions ────────────────────────────────────────────────────────

    private void AddPath(bool isDir)
    {
        string? p;
        if (isDir)
        {
            using var dlg = new FolderBrowserDialog();
            p = dlg.ShowDialog(this) == DialogResult.OK ? dlg.SelectedPath : null;
        }
        else
        {
            using var dlg = new OpenFileDialog();
            p = dlg.ShowDialog(this) == DialogResult.OK ? dlg.FileName : null;
        }
        if (p == null) return;

        if (isDir && !Directory.Exists(p))
        {
            MessageBox.Show(this, $"{p} is not a directory", "Invalid directory",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (!isDir && !File.Exists(p))
        {
            MessageBox.Show(this, $"{p} is not a file", "Invalid file",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var current = PathHelper.GetAppPaths(_appPath).Select(x => x.PathStr).ToList();

        if (current.Contains(p, StringComparer.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, $"The path\n'{p}'\nis already tracked.",
                "Duplicate Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!CheckPathConflicts(p, current, "Path")) return;

        PathHelper.SetAppPaths(_appPath, current.Append(p));
        RefreshList();
    }

    private void AddRegistryKey()
    {
        var entered = PromptForRegistryKey();
        if (string.IsNullOrWhiteSpace(entered)) return;

        string keyPath;
        try
        {
            keyPath = RegistryHelper.NormalizeKeyPath(entered);
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(this, ex.Message, "Invalid Registry Key",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!RegistryHelper.KeyExists(keyPath))
        {
            var answer = MessageBox.Show(this,
                $"The registry key\n'{keyPath}'\ndoes not currently exist.\n\nTrack it anyway?",
                "Registry Key Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;
        }

        var current = PathHelper.GetAppRegistryKeys(_appPath).Select(x => x.KeyPath).ToList();
        if (!CheckRegistryConflicts(keyPath, current)) return;

        PathHelper.SetAppRegistryKeys(_appPath, current.Append(keyPath));
        RefreshList();
    }

    private string? PromptForRegistryKey()
    {
        using var form = new Form
        {
            Text = "Add Registry Key",
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new System.Drawing.Size(520, 125),
        };

        var label = new Label
        {
            AutoSize = true,
            Location = new System.Drawing.Point(12, 12),
            Text = "Paste the registry key to track, for example HKCU\\Software\\Vendor\\Game.",
        };
        var textBox = new TextBox
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Location = new System.Drawing.Point(12, 38),
            Size = new System.Drawing.Size(496, 23),
        };
        var okButton = new Button
        {
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            DialogResult = DialogResult.OK,
            Location = new System.Drawing.Point(352, 86),
            Size = new System.Drawing.Size(75, 27),
            Text = "OK",
            UseVisualStyleBackColor = true,
        };
        var cancelButton = new Button
        {
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            DialogResult = DialogResult.Cancel,
            Location = new System.Drawing.Point(433, 86),
            Size = new System.Drawing.Size(75, 27),
            Text = "Cancel",
            UseVisualStyleBackColor = true,
        };

        form.Controls.Add(label);
        form.Controls.Add(textBox);
        form.Controls.Add(okButton);
        form.Controls.Add(cancelButton);
        form.AcceptButton = okButton;
        form.CancelButton = cancelButton;

        return form.ShowDialog(this) == DialogResult.OK ? textBox.Text : null;
    }

    private void RemovePath()
    {
        var sel = lstPaths.SelectedIndex;
        if (sel < 0) return;

        var paths = PathHelper.GetAppPaths(_appPath).Select(x => x.PathStr).ToList();
        if (sel < paths.Count)
        {
            paths.RemoveAt(sel);
            PathHelper.SetAppPaths(_appPath, paths);
            RefreshList();
            return;
        }

        var registryIndex = sel - paths.Count;
        var registryKeys = PathHelper.GetAppRegistryKeys(_appPath).Select(x => x.KeyPath).ToList();
        if (registryIndex >= 0 && registryIndex < registryKeys.Count)
        {
            registryKeys.RemoveAt(registryIndex);
            PathHelper.SetAppRegistryKeys(_appPath, registryKeys);
        }
        RefreshList();
    }
}
