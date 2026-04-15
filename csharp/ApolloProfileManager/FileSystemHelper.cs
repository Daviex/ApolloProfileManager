namespace ApolloProfileManager;

public static class FileSystemHelper
{
    /// <summary>
    /// Copies src to dst. Directories: rmtree dst then copytree (dirs_exist_ok).
    /// Files: ensures parent exists then copy2.
    /// </summary>
    public static void CopyItem(string src, string dst)
    {
        if (!File.Exists(src) && !Directory.Exists(src))
            return;

        if (Directory.Exists(src))
        {
            RemoveItem(dst);
            CopyDirectory(src, dst);
        }
        else
        {
            var parent = Path.GetDirectoryName(dst);
            if (!string.IsNullOrEmpty(parent))
                Directory.CreateDirectory(parent);
            File.Copy(src, dst, overwrite: true);
        }
    }

    /// <summary>Recursively copies a directory (equivalent to shutil.copytree with dirs_exist_ok=True).</summary>
    private static void CopyDirectory(string src, string dst)
    {
        Directory.CreateDirectory(dst);
        foreach (var file in Directory.GetFiles(src))
        {
            var destFile = Path.Combine(dst, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);
        }
        foreach (var dir in Directory.GetDirectories(src))
        {
            var destDir = Path.Combine(dst, Path.GetFileName(dir));
            CopyDirectory(dir, destDir);
        }
    }

    /// <summary>
    /// Removes a path. Directories: recursive delete. Files/symlinks: delete.
    /// Ignores if not found.
    /// </summary>
    public static void RemoveItem(string path)
    {
        try
        {
            if (Directory.Exists(path) && !IsSymlink(path))
                Directory.Delete(path, recursive: true);
            else if (File.Exists(path) || IsSymlink(path))
                File.Delete(path);
        }
        catch { /* ignore_errors equivalent */ }
    }

    private static bool IsSymlink(string path) =>
        (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
}
