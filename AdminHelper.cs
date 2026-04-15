using System.Security.Principal;

namespace ApolloProfileManager;

public static class AdminHelper
{
    /// <summary>Returns true if the current process is running with elevated (admin) privileges.</summary>
    public static bool IsElevated()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }
}
