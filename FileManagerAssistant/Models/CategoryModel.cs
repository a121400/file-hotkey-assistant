namespace FileManagerAssistant.Models;

public class CategoryModel
{
    public string Name { get; set; } = string.Empty;
    public List<ShortcutItem> Items { get; set; } = new();
}

public class AppConfig
{
    public List<CategoryModel> Categories { get; set; } = new();
    public bool AutoStart { get; set; } = false;
    public int SelectedCategoryIndex { get; set; } = 0;
}
