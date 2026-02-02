using System.Text.Json.Serialization;

namespace FileManagerAssistant.Models;

public enum ItemType
{
    Folder,
    Application
}

public class ShortcutItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public ItemType Type { get; set; }

    [JsonIgnore]
    public object? Icon { get; set; }
}
