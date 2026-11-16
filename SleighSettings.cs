using System.Text.Json;

namespace DesktopSleigh;

internal sealed class SleighSettings
{
    private static readonly string SettingsDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DesktopSleigh");

    public int Size { get; set; } = 32;
    public int Speed { get; set; } = 28;
    public bool SoundEnabled { get; set; } = true;
    public int VolumeDb { get; set; }
    public bool AvoidTaskbar { get; set; } = true;
    public string? ScreenName { get; set; }

    public static SleighSettings Load()
    {
        string path = Path.Combine(SettingsDirectory, "settings.json");
        if (!File.Exists(path))
        {
            return new();
        }

        try
        {
            SleighSettings settings = JsonSerializer.Deserialize<SleighSettings>(File.ReadAllText(path))
                ?? throw new JsonException("The settings file is empty.");
            if (settings.Size is not (24 or 32 or 48) || settings.Speed is not (16 or 28 or 48)
                || settings.VolumeDb is not (0 or -3 or -30))
            {
                throw new JsonException("The saved sleigh size, speed or volume is invalid.");
            }

            return settings;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            MessageBox.Show($"Couldn't load DesktopSleigh's options. Default options will be used.\n\n{exception.Message}",
                "DesktopSleigh", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return new();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            string path = Path.Combine(SettingsDirectory, "settings.json");
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporary, path, overwrite: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Couldn't save DesktopSleigh's options.\n\n{exception.Message}",
                "DesktopSleigh", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
