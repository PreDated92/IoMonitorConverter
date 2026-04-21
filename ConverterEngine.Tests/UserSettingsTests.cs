using ConverterEngine;

namespace ConverterEngine.Tests;

public class UserSettingsTests
{
    [Fact]
    public async Task Save_CreatesSettingsFile()
    {
        var settings = new UserSettings
        {
            LastXmlFolderPath = @"C:\TestFolder"
        };

        settings.Save();

        await Task.Delay(100);

        var loadedSettings = UserSettings.Load();
        Assert.Equal(@"C:\TestFolder", loadedSettings.LastXmlFolderPath);
    }

    [Fact]
    public void Load_WithMissingFile_ReturnsDefaultSettings()
    {
        var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        if (File.Exists(settingsPath))
        {
            File.Delete(settingsPath);
        }

        var settings = UserSettings.Load();

        Assert.NotNull(settings);
        Assert.Equal(string.Empty, settings.LastXmlFolderPath);
    }

    [Fact]
    public void Load_WithValidFile_ReturnsSettings()
    {
        var testSettings = new UserSettings
        {
            LastXmlFolderPath = @"C:\ValidFolder"
        };
        testSettings.Save();

        var loadedSettings = UserSettings.Load();

        Assert.NotNull(loadedSettings);
        Assert.Equal(@"C:\ValidFolder", loadedSettings.LastXmlFolderPath);
    }

    [Fact]
    public void Load_WithCorruptedJson_ReturnsDefaultSettings()
    {
        var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        File.WriteAllText(settingsPath, "{ invalid json }");

        var settings = UserSettings.Load();

        Assert.NotNull(settings);
        Assert.Equal(string.Empty, settings.LastXmlFolderPath);
    }

    [Fact]
    public void LastXmlFolderPath_Persistence_WorksCorrectly()
    {
        var testPath = @"C:\MyDocuments\XmlFiles";
        var settings = new UserSettings
        {
            LastXmlFolderPath = testPath
        };
        settings.Save();

        var reloaded = UserSettings.Load();

        Assert.Equal(testPath, reloaded.LastXmlFolderPath);
    }
}
