using System.Drawing;
using System.Windows;
using FileManagerAssistant.Services;
using Forms = System.Windows.Forms;

namespace FileManagerAssistant;

public partial class App : System.Windows.Application
{
    private MouseHookService? _mouseHook;
    private Forms.NotifyIcon? _notifyIcon;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 创建主窗口（但不显示）
        _mainWindow = new MainWindow();

        // 初始化鼠标钩子（双击中键触发）
        _mouseHook = new MouseHookService();
        _mouseHook.MiddleButtonDoubleClicked += OnMiddleButtonDoubleClicked;
        _mouseHook.Start();

        // 初始化系统托盘
        InitializeNotifyIcon();
    }

    private void InitializeNotifyIcon()
    {
        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "文件管理助手",
            Visible = true
        };

        // 尝试加载图标，如果没有则使用默认
        try
        {
            var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "icon.ico");
            if (System.IO.File.Exists(iconPath))
            {
                _notifyIcon.Icon = new Icon(iconPath);
            }
            else
            {
                _notifyIcon.Icon = SystemIcons.Application;
            }
        }
        catch
        {
            _notifyIcon.Icon = SystemIcons.Application;
        }

        // 托盘右键菜单
        var contextMenu = new Forms.ContextMenuStrip();

        var showItem = new Forms.ToolStripMenuItem("显示窗口");
        showItem.Click += (s, e) => _mainWindow?.ToggleVisibility();

        var autoStartItem = new Forms.ToolStripMenuItem("开机自启");
        autoStartItem.CheckOnClick = true;
        autoStartItem.Checked = AutoStartService.IsAutoStartEnabled();
        autoStartItem.Click += (s, e) =>
        {
            if (autoStartItem.Checked)
                AutoStartService.EnableAutoStart();
            else
                AutoStartService.DisableAutoStart();
        };

        var exitItem = new Forms.ToolStripMenuItem("退出");
        exitItem.Click += (s, e) => Shutdown();

        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(autoStartItem);
        contextMenu.Items.Add(new Forms.ToolStripSeparator());
        contextMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;

        // 双击托盘图标显示窗口
        _notifyIcon.DoubleClick += (s, e) => _mainWindow?.ToggleVisibility();
    }

    private void OnMiddleButtonDoubleClicked()
    {
        Dispatcher.Invoke(() => _mainWindow?.ToggleVisibility());
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mouseHook?.Dispose();
        _notifyIcon?.Dispose();
        base.OnExit(e);
    }
}
