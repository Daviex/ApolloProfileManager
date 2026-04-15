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
    private void BtnRemove_Click(object? sender, EventArgs e)  => RemovePath();
    private void BtnClose_Click(object? sender, EventArgs e)   => Close();

    // ── List refresh ──────────────────────────────────────────────────────────

    private void RefreshList()
    {
        lstPaths.BeginUpdate();
        lstPaths.Items.Clear();
        foreach (var (pStr, _) in PathHelper.GetAppPaths(_appPath))
            lstPaths.Items.Add(pStr);
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

    private void RemovePath()
    {
        var sel = lstPaths.SelectedIndex;
        if (sel < 0) return;
        var paths = PathHelper.GetAppPaths(_appPath).Select(x => x.PathStr).ToList();
        paths.RemoveAt(sel);
        PathHelper.SetAppPaths(_appPath, paths);
        RefreshList();
    }
}
