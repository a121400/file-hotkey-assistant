# 文件快捷键助手

一个 Windows 桌面工具，通过双击鼠标中键全局唤出悬浮菜单，快速访问常用目录和应用程序。

## 功能特性

- **全局鼠标中键双击监听** - 任何窗口下双击中键都能触发/隐藏悬浮菜单
- **悬浮菜单** - 网格布局显示图标和名称
- **分类管理** - 左侧分类栏，支持多个分类
- **快速跳转** - 点击目录项在资源管理器中打开
- **快速启动** - 点击应用项直接运行程序
- **拖拽添加** - 直接拖拽 exe/文件夹到窗口即可添加
- **自动图标** - 自动提取 exe 和文件夹的图标显示
- **系统托盘** - 最小化到托盘，右键菜单管理
- **开机自启** - 程序随 Windows 启动自动运行

## 截图

![主界面](https://via.placeholder.com/600x400?text=Screenshot)

## 安装使用

### 方式一：下载发布版本（推荐）

1. 前往 [Releases](https://github.com/a121400/file-hotkey-assistant/releases) 页面
2. 下载最新版本的 `FileManagerAssistant-vX.X.X-win-x64.zip`
3. 解压到任意目录
4. 运行 `FileManagerAssistant.exe`

### 方式二：从源码编译

```bash
# 克隆仓库
git clone https://github.com/a121400/file-hotkey-assistant.git

# 进入目录
cd file-hotkey-assistant

# 编译运行
dotnet run --project FileManagerAssistant
```

## 使用方法

1. 启动程序后，程序会最小化到系统托盘
2. 在任意窗口下**双击鼠标中键**，唤出悬浮菜单
3. 再次双击中键或点击其他区域，隐藏菜单
4. **拖拽文件夹或 exe 文件**到窗口中即可添加快捷方式
5. **右键点击项目**可以删除或重命名
6. **右键点击托盘图标**可以设置开机自启或退出程序

## 技术栈

- C# / .NET 9
- WPF (Windows Presentation Foundation)
- Win32 API (全局鼠标钩子)

## 系统要求

- Windows 10/11
- 无需安装 .NET 运行时（发布版本已包含）

## 许可证

MIT License
