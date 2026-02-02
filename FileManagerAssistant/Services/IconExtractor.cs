using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FileManagerAssistant.Helpers;

namespace FileManagerAssistant.Services;

public static class IconExtractor
{
    private static readonly Dictionary<string, ImageSource> IconCache = new();

    public static ImageSource? GetIcon(string path)
    {
        if (string.IsNullOrEmpty(path) || !Path.Exists(path))
            return null;

        if (IconCache.TryGetValue(path, out var cached))
            return cached;

        try
        {
            ImageSource? icon = null;

            // 尝试使用 SHGetFileInfo 获取图标
            var shinfo = new Win32Api.SHFILEINFO();
            var result = Win32Api.SHGetFileInfo(
                path,
                0,
                ref shinfo,
                (uint)System.Runtime.InteropServices.Marshal.SizeOf(shinfo),
                Win32Api.SHGFI_ICON | Win32Api.SHGFI_LARGEICON);

            if (result != IntPtr.Zero && shinfo.hIcon != IntPtr.Zero)
            {
                icon = Imaging.CreateBitmapSourceFromHIcon(
                    shinfo.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                Win32Api.DestroyIcon(shinfo.hIcon);
            }

            // 备用方法：使用 ExtractAssociatedIcon
            if (icon == null && File.Exists(path))
            {
                using var ico = Icon.ExtractAssociatedIcon(path);
                if (ico != null)
                {
                    icon = Imaging.CreateBitmapSourceFromHIcon(
                        ico.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }

            if (icon != null)
            {
                icon.Freeze();
                IconCache[path] = icon;
            }

            return icon;
        }
        catch
        {
            return null;
        }
    }

    public static void ClearCache()
    {
        IconCache.Clear();
    }
}
