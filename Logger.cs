namespace ApolloProfileManager;

static class Logger
{
    private static StreamWriter? _logWriter;

    /// <summary>
    /// Redirects Console.Out and Console.Error to tee writers that write to both the
    /// original streams and a daily log file under %LOCALAPPDATA%\ApolloProfileManager\logs\.
    /// </summary>
    public static void Initialize()
    {
        var logsDir = Path.Combine(PathHelper.GetAppDataDir(), "logs");
        Directory.CreateDirectory(logsDir);
        var logPath = Path.Combine(logsDir, $"manager-{DateTime.Now:yyyy-MM-dd}.log");
        _logWriter = new StreamWriter(logPath, append: true, System.Text.Encoding.UTF8) { AutoFlush = true };

        Console.SetOut(new TeeTextWriter(Console.Out, _logWriter, "OUT"));
        Console.SetError(new TeeTextWriter(Console.Error, _logWriter, "ERR"));
    }

    public static void Close()
    {
        _logWriter?.Flush();
        _logWriter?.Close();
        _logWriter = null;
    }

    private sealed class TeeTextWriter(TextWriter console, TextWriter file, string tag) : TextWriter
    {
        public override System.Text.Encoding Encoding => console.Encoding;

        public override void Write(char value)
        {
            console.Write(value);
            file.Write(value);
        }

        public override void Write(string? value)
        {
            console.Write(value);
            file.Write(value);
        }

        public override void WriteLine(string? value)
        {
            console.WriteLine(value);
            file.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{tag}] {value}");
        }

        public override void WriteLine()
        {
            console.WriteLine();
            file.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{tag}]");
        }

        public override void Flush()
        {
            console.Flush();
            file.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Flush();
            base.Dispose(disposing);
        }
    }
}
