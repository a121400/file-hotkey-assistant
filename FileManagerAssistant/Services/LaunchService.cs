using System.Diagnostics;
using System.IO;
using FileManagerAssistant.Models;

namespace FileManagerAssistant.Services;

public static class LaunchService
{
    public static void Launch(ShortcutItem item)
    {
        if (string.IsNullOrEmpty(item.Path) || !Path.Exists(item.Path))
            return;

        try
        {
            if (item.Type == ItemType.Folder)
            {
                // 打开文件夹
                Process.Start("explorer.exe", item.Path);
            }
            else
            {
                // 打开应用程序
                var startInfo = new ProcessStartInfo
                {
                    FileName = item.Path,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(item.Path) ?? ""
                };
                Process.Start(startInfo);
            }
        }
        catch
        {
            // 忽略启动错误
        }
    }
}
