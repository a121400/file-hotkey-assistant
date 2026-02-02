using System.IO;
using System.Text.Json;
using FileManagerAssistant.Models;

namespace FileManagerAssistant.Services;

public class ConfigService
{
    private readonly string _configPath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public AppConfig Config { get; private set; } = new();

    public ConfigService()
    {
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        _configPath = Path.Combine(appDir, "config.json");
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                Config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
            }
        }
        catch
        {
            Config = new AppConfig();
        }

        // 确保至少有一个默认分类
        if (Config.Categories.Count == 0)
        {
            Config.Categories.Add(new CategoryModel { Name = "默认分类" });
        }
    }

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(Config, JsonOptions);
            File.WriteAllText(_configPath, json);
        }
        catch
        {
            // 忽略保存错误
        }
    }

    public void AddCategory(string name)
    {
        Config.Categories.Add(new CategoryModel { Name = name });
        Save();
    }

    public void RemoveCategory(int index)
    {
        if (index >= 0 && index < Config.Categories.Count && Config.Categories.Count > 1)
        {
            Config.Categories.RemoveAt(index);
            if (Config.SelectedCategoryIndex >= Config.Categories.Count)
            {
                Config.SelectedCategoryIndex = Config.Categories.Count - 1;
            }
            Save();
        }
    }

    public void AddItem(int categoryIndex, ShortcutItem item)
    {
        if (categoryIndex >= 0 && categoryIndex < Config.Categories.Count)
        {
            Config.Categories[categoryIndex].Items.Add(item);
            Save();
        }
    }

    public void RemoveItem(int categoryIndex, int itemIndex)
    {
        if (categoryIndex >= 0 && categoryIndex < Config.Categories.Count)
        {
            var category = Config.Categories[categoryIndex];
            if (itemIndex >= 0 && itemIndex < category.Items.Count)
            {
                category.Items.RemoveAt(itemIndex);
                Save();
            }
        }
    }
}
