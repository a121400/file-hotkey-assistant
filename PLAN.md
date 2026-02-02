# 文件管理助手 - 实现计划

## 功能概述
一个 Windows 桌面工具，通过鼠标中键全局唤出悬浮菜单，快速访问常用目录和应用程序。

## 核心功能
1. **全局鼠标中键双击监听** - 任何窗口下双击中键都能触发/隐藏
2. **悬浮菜单** - 网格布局显示图标和名称
3. **分类管理** - 左侧分类栏，支持多个分类
4. **快速跳转** - 点击目录项在资源管理器中打开
5. **快速启动** - 点击应用项直接运行程序
6. **拖拽添加** - 直接拖拽 exe/文件夹到窗口即可添加
7. **自动图标** - 自动提取 exe 和文件夹的图标显示
8. **系统托盘** - 最小化到托盘，右键菜单管理
9. **开机自启** - 程序随 Windows 启动自动运行

## UI 风格（参考图片）
- **绿色主题** - 浅绿色背景
- 圆角边框窗口
- 左侧：分类列表栏
- 右侧：网格布局图标区域（6-7列）
- 右上角：快捷键提示 + 关闭按钮
- 每个项目：图标 + 名称（居中显示）
- 悬停高亮效果

## 技术方案

### 技术栈
- **语言**: C# (.NET 6/7)
- **UI框架**: WPF
- **鼠标钩子**: Win32 API (SetWindowsHookEx)
- **配置存储**: JSON 文件

### 项目结构
```
文件管理助手/
├── FileManagerAssistant.sln          # 解决方案文件
├── FileManagerAssistant/
│   ├── App.xaml                      # 应用程序入口
│   ├── App.xaml.cs
│   ├── MainWindow.xaml               # 悬浮菜单窗口（主界面）
│   ├── MainWindow.xaml.cs
│   ├── Models/
│   │   ├── CategoryModel.cs          # 分类数据模型
│   │   └── ShortcutItem.cs           # 快捷项数据模型
│   ├── Services/
│   │   ├── MouseHookService.cs       # 全局鼠标钩子
│   │   ├── ConfigService.cs          # 配置读写（JSON）
│   │   ├── IconExtractor.cs          # 提取exe/文件夹图标
│   │   └── LaunchService.cs          # 启动目录/应用
│   ├── Helpers/
│   │   └── Win32Api.cs               # Win32 API 声明
│   └── Resources/
│       └── icon.ico                  # 托盘图标
└── config.json                       # 用户配置文件
```

## 实现步骤

### 步骤 1: 创建项目基础
- 创建 WPF 项目 (.NET 6+)
- 设置项目结构和基本文件

### 步骤 2: 实现全局鼠标钩子
- 使用 `SetWindowsHookEx` 和 `WH_MOUSE_LL`
- 监听鼠标中键按下事件
- 检测双击中键（500ms内连续两次点击）
- 双击中键切换窗口显示/隐藏

### 步骤 3: 创建主窗口 UI
- 无边框窗口 (`WindowStyle="None"`)
- 绿色主题背景
- 左侧：分类列表（ListBox）
- 右侧：网格图标区域（WrapPanel/UniformGrid）
- 右上角：快捷键提示 + 关闭按钮
- 支持拖拽文件进窗口

### 步骤 4: 实现数据模型
```csharp
public class CategoryModel
{
    public string Name { get; set; }              // 分类名称
    public List<ShortcutItem> Items { get; set; } // 该分类下的项目
}

public class ShortcutItem
{
    public string Name { get; set; }      // 显示名称
    public string Path { get; set; }      // 文件/目录路径
    public ItemType Type { get; set; }    // Folder 或 Application
}
```

### 步骤 5: 实现图标提取服务
- 使用 `SHGetFileInfo` API 提取 exe 图标
- 使用 Shell32 提取文件夹图标
- 缓存图标避免重复提取

### 步骤 6: 实现拖拽添加功能
- 窗口 `AllowDrop="True"`
- 处理 `Drop` 事件
- 自动识别 exe 文件或文件夹
- 自动提取名称和图标

### 步骤 7: 实现配置服务
- JSON 序列化/反序列化
- 配置文件存储在程序目录
- 自动保存修改

### 步骤 8: 实现启动服务
- 文件夹：`Process.Start("explorer.exe", path)`
- 应用：`Process.Start(path)`

### 步骤 9: 右键菜单功能
- 右键项目弹出菜单
- 选项：打开、删除、重命名

### 步骤 10: 系统托盘功能
- 使用 `NotifyIcon`
- 右键菜单：显示、退出
- 双击显示主窗口

### 步骤 11: 开机自启
- 使用注册表 `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`

## 关键代码示例

### 鼠标钩子核心逻辑（双击中键检测）
```csharp
// 双击检测相关字段
private DateTime _lastMiddleClickTime = DateTime.MinValue;
private int _clickCount = 0;
private const int DoubleClickThresholdMs = 500; // 双击时间阈值（毫秒）

private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
{
    if (nCode >= 0 && wParam == (IntPtr)WM_MBUTTONDOWN)
    {
        var now = DateTime.Now;
        var elapsed = (now - _lastMiddleClickTime).TotalMilliseconds;

        if (elapsed <= DoubleClickThresholdMs)
        {
            _clickCount++;
            if (_clickCount >= 2)
            {
                MiddleButtonDoubleClicked?.Invoke();
                _clickCount = 0;
                _lastMiddleClickTime = DateTime.MinValue;
            }
        }
        else
        {
            _clickCount = 1;
        }
        _lastMiddleClickTime = now;
    }
    return CallNextHookEx(_hookID, nCode, wParam, lParam);
}
```

### 拖拽添加
```csharp
private void Window_Drop(object sender, DragEventArgs e)
{
    if (e.Data.GetDataPresent(DataFormats.FileDrop))
    {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        foreach (string file in files)
        {
            AddShortcut(file);
        }
    }
}
```

### 提取图标
```csharp
public static ImageSource GetIcon(string path)
{
    using var icon = System.Drawing.Icon.ExtractAssociatedIcon(path);
    return Imaging.CreateBitmapSourceFromHIcon(
        icon.Handle, Int32Rect.Empty,
        BitmapSizeOptions.FromEmptyOptions());
}
```

## 验证方案
1. 编译运行程序
2. 在任意窗口双击鼠标中键，确认悬浮窗弹出
3. 再次双击中键，确认悬浮窗隐藏
4. 添加一个测试目录，点击确认能打开资源管理器
5. 添加一个测试应用（如记事本），点击确认能启动
6. 关闭程序重启，确认配置已保存

## 注意事项
- 全局钩子需要管理员权限才能在所有窗口生效
- 程序退出时必须卸载钩子，否则可能影响系统
- 悬浮窗需要处理多显示器场景
