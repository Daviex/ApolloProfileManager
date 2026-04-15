using System.Diagnostics;
using System.Text;

namespace ApolloProfileManager;

public static class ErrorDialogHelper
{
    /// <summary>
    /// Base64-encodes errorMessage, spawns a detached instance of this exe with
    /// --show-error-dialog, then exits the current process with code 0.
    /// Mirrors Python's _spawn_error_dialog_and_exit_zero().
    /// </summary>
    public static void SpawnErrorDialogAndExitZero(string errorMessage)
    {
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(errorMessage));
        var exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = $"--show-error-dialog {encoded}",
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        try
        {
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FATAL: Failed to spawn error dialog process: {ex.Message}");
            Console.Error.WriteLine($"Original error was:\n{errorMessage}");
            Environment.Exit(1);
        }

        Environment.Exit(0);
    }

    /// <summary>
    /// Decodes a base64-encoded error message and shows it in a MessageBox.
    /// Called when the app is launched with --show-error-dialog.
    /// </summary>
    public static void ShowErrorDialog(string encodedMessage)
    {
        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encodedMessage));
            MessageBox.Show(decoded, "Apollo Profile Manager - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error within --show-error-dialog handler: {ex.Message}");
            Environment.Exit(1);
        }
        Environment.Exit(0);
    }
}
