using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FileManagerAssistant.Models;
using FileManagerAssistant.Services;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfColor = System.Windows.Media.Color;
using WpfOrientation = System.Windows.Controls.Orientation;

namespace FileManagerAssistant;

public partial class MainWindow : Window
{
    private readonly ConfigService _configService;
    private ContextMenu? _itemContextMenu;

    public MainWindow()
    {
        InitializeComponent();
        _configService = new ConfigService();
        _configService.Load();

        InitializeContextMenu();
        LoadCategories();

        // 窗口失去焦点时隐藏
        Deactivated += (s, e) => Hide();
    }

    private void InitializeContextMenu()
    {
        _itemContextMenu = new ContextMenu();
        var deleteItem = new MenuItem { Header = "删除" };
        deleteItem.Click += DeleteItem_Click;
        var renameItem = new MenuItem { Header = "重命名" };
        renameItem.Click += RenameItem_Click;
        _itemContextMenu.Items.Add(deleteItem);
        _itemContextMenu.Items.Add(renameItem);
    }

    private void LoadCategories()
    {
        CategoryList.ItemsSource = _configService.Config.Categories;
        if (_configService.Config.Categories.Count > 0)
        {
            CategoryList.SelectedIndex = _configService.Config.SelectedCategoryIndex;
        }
    }

    private void RefreshItems()
    {
        if (CategoryList.SelectedItem is CategoryModel category)
        {
            // 加载图标
            foreach (var item in category.Items)
            {
                item.Icon ??= IconExtractor.GetIcon(item.Path);
            }
            ItemsPanel.ItemsSource = null;
            ItemsPanel.ItemsSource = category.Items;

            // 更新空状态显示
            UpdateEmptyState(category.Items.Count == 0);
        }
    }

    private void UpdateEmptyState(bool isEmpty)
    {
        EmptyStatePanel.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
    }

    public void ToggleVisibility()
    {
        if (IsVisible)
        {
            Hide();
        }
        else
        {
            ShowAtCursor();
        }
    }

    private void ShowAtCursor()
    {
        var (x, y) = MouseHookService.GetCursorPosition();

        // 获取屏幕尺寸
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;

        // 确保窗口不超出屏幕边界
        if (x + Width > screenWidth) x = (int)(screenWidth - Width);
        if (y + Height > screenHeight) y = (int)(screenHeight - Height);
        if (x < 0) x = 0;
        if (y < 0) y = 0;

        Left = x;
        Top = y;
        Show();
        Activate();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void BtnAddCategory_Click(object sender, RoutedEventArgs e)
    {
        var name = ShowModernInputDialog("添加分类", "请输入分类名称:", "新分类");
        if (!string.IsNullOrWhiteSpace(name))
        {
            _configService.AddCategory(name);
            LoadCategories();
            CategoryList.SelectedIndex = _configService.Config.Categories.Count - 1;
        }
    }

    private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoryList.SelectedIndex >= 0)
        {
            _configService.Config.SelectedCategoryIndex = CategoryList.SelectedIndex;
            _configService.Save();
            RefreshItems();
        }
    }

    private void DeleteCategory_Click(object sender, RoutedEventArgs e)
    {
        if (CategoryList.SelectedIndex >= 0 && _configService.Config.Categories.Count > 1)
        {
            var result = System.Windows.MessageBox.Show("确定要删除这个分类吗?", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _configService.RemoveCategory(CategoryList.SelectedIndex);
                LoadCategories();
            }
        }
        else if (_configService.Config.Categories.Count <= 1)
        {
            System.Windows.MessageBox.Show("至少需要保留一个分类", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void RenameCategory_Click(object sender, RoutedEventArgs e)
    {
        if (CategoryList.SelectedItem is CategoryModel category)
        {
            var name = ShowModernInputDialog("重命名分类", "请输入新名称:", category.Name);
            if (!string.IsNullOrWhiteSpace(name))
            {
                category.Name = name;
                _configService.Save();
                LoadCategories();
            }
        }
    }

    private void IconArea_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            e.Effects = System.Windows.DragDropEffects.Copy;
        }
        else
        {
            e.Effects = System.Windows.DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void IconArea_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop)!;
            foreach (var file in files)
            {
                AddShortcut(file);
            }
        }
    }

    private void AddShortcut(string path)
    {
        if (!Path.Exists(path)) return;

        var isDirectory = Directory.Exists(path);
        var name = Path.GetFileNameWithoutExtension(path);
        if (isDirectory) name = Path.GetFileName(path);

        var item = new ShortcutItem
        {
            Name = name,
            Path = path,
            Type = isDirectory ? ItemType.Folder : ItemType.Application,
            Icon = IconExtractor.GetIcon(path)
        };

        _configService.AddItem(CategoryList.SelectedIndex, item);
        RefreshItems();
    }

    private void ShortcutItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is ShortcutItem item)
        {
            LaunchService.Launch(item);
            Hide();
        }
    }

    private void ShortcutItem_RightClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && _itemContextMenu != null)
        {
            _itemContextMenu.Tag = btn.Tag;
            _itemContextMenu.IsOpen = true;
        }
    }

    private void DeleteItem_Click(object sender, RoutedEventArgs e)
    {
        if (_itemContextMenu?.Tag is ShortcutItem item && CategoryList.SelectedItem is CategoryModel category)
        {
            var index = category.Items.IndexOf(item);
            if (index >= 0)
            {
                _configService.RemoveItem(CategoryList.SelectedIndex, index);
                RefreshItems();
            }
        }
    }

    private void RenameItem_Click(object sender, RoutedEventArgs e)
    {
        if (_itemContextMenu?.Tag is ShortcutItem item)
        {
            var name = ShowModernInputDialog("重命名", "请输入新名称:", item.Name);
            if (!string.IsNullOrWhiteSpace(name))
            {
                item.Name = name;
                _configService.Save();
                RefreshItems();
            }
        }
    }

    private string? ShowModernInputDialog(string title, string message, string defaultValue = "")
    {
        var dialog = new Window
        {
            Title = title,
            Width = 360,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = WpfBrushes.Transparent
        };

        // 主容器
        var mainBorder = new Border
        {
            Background = new System.Windows.Media.SolidColorBrush(WpfColor.FromRgb(250, 250, 250)),
            CornerRadius = new CornerRadius(12),
            BorderBrush = new System.Windows.Media.SolidColorBrush(WpfColor.FromRgb(92, 107, 192)),
            BorderThickness = new Thickness(1),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 20,
                ShadowDepth = 5,
                Opacity = 0.2
            }
        };

        var mainGrid = new Grid();
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // 标题栏
        var titleBar = new Border
        {
            Background = new System.Windows.Media.LinearGradientBrush(
                WpfColor.FromRgb(63, 81, 181),
                WpfColor.FromRgb(121, 134, 203),
                0),
            CornerRadius = new CornerRadius(11, 11, 0, 0)
        };
        titleBar.MouseLeftButtonDown += (s, e) => dialog.DragMove();

        var titleGrid = new Grid();
        var titleText = new TextBlock
        {
            Text = title,
            Foreground = WpfBrushes.White,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(16, 0, 0, 0)
        };
        titleGrid.Children.Add(titleText);
        titleBar.Child = titleGrid;
        Grid.SetRow(titleBar, 0);

        // 内容区域
        var contentPanel = new StackPanel
        {
            Margin = new Thickness(20, 15, 20, 15),
            VerticalAlignment = VerticalAlignment.Center
        };

        var label = new TextBlock
        {
            Text = message,
            FontSize = 13,
            Foreground = new System.Windows.Media.SolidColorBrush(WpfColor.FromRgb(117, 117, 117)),
            Margin = new Thickness(0, 0, 0, 10)
        };

        var textBox = new System.Windows.Controls.TextBox
        {
            Text = defaultValue,
            FontSize = 14,
            Padding = new Thickness(10, 8, 10, 8),
            BorderBrush = new System.Windows.Media.SolidColorBrush(WpfColor.FromRgb(197, 202, 233)),
            BorderThickness = new Thickness(1),
            Background = WpfBrushes.White
        };

        // 按钮面板
        var btnPanel = new StackPanel
        {
            Orientation = WpfOrientation.Horizontal,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
            Margin = new Thickness(0, 15, 0, 0)
        };

        var okBtn = new System.Windows.Controls.Button
        {
            Content = "确定",
            Width = 70,
            Height = 32,
            Margin = new Thickness(0, 0, 10, 0),
            Background = new System.Windows.Media.SolidColorBrush(WpfColor.FromRgb(92, 107, 192)),
            Foreground = WpfBrushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand
        };

        var cancelBtn = new System.Windows.Controls.Button
        {
            Content = "取消",
            Width = 70,
            Height = 32,
            Background = new System.Windows.Media.SolidColorBrush(WpfColor.FromRgb(238, 238, 238)),
            Foreground = new System.Windows.Media.SolidColorBrush(WpfColor.FromRgb(97, 97, 97)),
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand
        };

        okBtn.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };
        cancelBtn.Click += (s, e) => { dialog.DialogResult = false; dialog.Close(); };

        btnPanel.Children.Add(okBtn);
        btnPanel.Children.Add(cancelBtn);
        contentPanel.Children.Add(label);
        contentPanel.Children.Add(textBox);
        contentPanel.Children.Add(btnPanel);
        Grid.SetRow(contentPanel, 1);

        mainGrid.Children.Add(titleBar);
        mainGrid.Children.Add(contentPanel);
        mainBorder.Child = mainGrid;
        dialog.Content = mainBorder;

        textBox.SelectAll();
        textBox.Focus();

        // 回车确认
        textBox.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter)
            {
                dialog.DialogResult = true;
                dialog.Close();
            }
            else if (e.Key == Key.Escape)
            {
                dialog.DialogResult = false;
                dialog.Close();
            }
        };

        return dialog.ShowDialog() == true ? textBox.Text : null;
    }
}
