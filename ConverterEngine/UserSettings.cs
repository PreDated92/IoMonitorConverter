using System.IO;
using System.Text.Json;

namespace ConverterEngine;

public class UserSettings
{
    public string LastXmlFolderPath { get; set; } = string.Empty;

    private static string GetSettingsPath()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(appDirectory, "settings.json");
    }

    public static UserSettings Load()
    {
        try
        {
            var settingsPath = GetSettingsPath();
            
            if (!File.Exists(settingsPath))
            {
                return new UserSettings();
            }

            var json = File.ReadAllText(settingsPath);
            return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
        }
        catch (Exception)
        {
            return new UserSettings();
        }
    }

    public void Save()
    {
        try
        {
            var settingsPath = GetSettingsPath();
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(settingsPath, json);
        }
        catch (Exception)
        {
        }
    }
}
