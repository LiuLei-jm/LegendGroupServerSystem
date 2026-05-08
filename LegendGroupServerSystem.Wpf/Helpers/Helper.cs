using System.IO;

namespace LegendGroupServerSystem.WPf.Helpers;

public static class Helper
{
    public static string GetExeCurrentPath()
    {
        string originalExePath = Environment.ProcessPath!;
        if (originalExePath != null)
        {
            return Path.GetDirectoryName(originalExePath) ?? AppContext.BaseDirectory;
        }
        return AppContext.BaseDirectory;
    }

}
