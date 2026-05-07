using System.IO.Compression;

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

    /// <summary>Recursively copies a directory (equivalent to shutil.copytree with dirs_exist_ok=True, ignore_errors=True).</summary>
    private static void CopyDirectory(string src, string dst)
    {
        Directory.CreateDirectory(dst);
        foreach (var file in Directory.GetFiles(src))
        {
            try
            {
                var destFile = Path.Combine(dst, Path.GetFileName(file));
                if (File.Exists(destFile))
                    File.SetAttributes(destFile, FileAttributes.Normal);
                File.Copy(file, destFile, overwrite: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[warn] Failed to copy file '{file}' to '{dst}': {ex.Message}");
            }
        }
        foreach (var dir in Directory.GetDirectories(src))
        {
            try
            {
                var destDir = Path.Combine(dst, Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[warn] Failed to copy directory '{dir}' to '{dst}': {ex.Message}");
            }
        }
    }

    /// <summary>Creates a zip archive from a directory, skipping inaccessible files/dirs.</summary>
    public static void ZipDirectory(string srcDir, string dstZip)
    {
        var parent = Path.GetDirectoryName(dstZip);
        if (!string.IsNullOrEmpty(parent))
            Directory.CreateDirectory(parent);
        using var zip = ZipFile.Open(dstZip, ZipArchiveMode.Create);
        AddToZip(zip, srcDir, "");
    }

    private static void AddToZip(ZipArchive zip, string srcDir, string entryPrefix)
    {
        foreach (var file in Directory.GetFiles(srcDir))
        {
            try
            {
                zip.CreateEntryFromFile(file, entryPrefix + Path.GetFileName(file));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[warn] Failed to zip file '{file}': {ex.Message}");
            }
        }
        foreach (var dir in Directory.GetDirectories(srcDir))
        {
            try
            {
                var dirName = Path.GetFileName(dir);
                AddToZip(zip, dir, entryPrefix + dirName + "/");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[warn] Failed to zip directory '{dir}': {ex.Message}");
            }
        }
    }

    /// <summary>Extracts a zip archive to a directory, skipping files that can't be written.</summary>
    public static void UnzipTo(string srcZip, string dstDir)
    {
        Directory.CreateDirectory(dstDir);
        using var zip = ZipFile.OpenRead(srcZip);
        foreach (var entry in zip.Entries)
        {
            try
            {
                var destPath = Path.Combine(dstDir, entry.FullName.Replace('/', Path.DirectorySeparatorChar));
                if (string.IsNullOrEmpty(entry.Name))
                {
                    Directory.CreateDirectory(destPath);
                }
                else
                {
                    var parent = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(parent))
                        Directory.CreateDirectory(parent);
                    if (File.Exists(destPath))
                        File.SetAttributes(destPath, FileAttributes.Normal);
                    entry.ExtractToFile(destPath, overwrite: true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[warn] Failed to extract '{entry.FullName}': {ex.Message}");
            }
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
