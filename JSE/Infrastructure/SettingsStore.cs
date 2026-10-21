using System;
using System.IO;
using System.Text.Json;
using JSE.Core;

namespace JSE.Infrastructure
{
    public static class SettingsStore
    {
        private static readonly string Folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JSE", "AssistDriftCarFH5");
        public static string FilePath => Path.Combine(Folder, "settings.json");

        public static AssistSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var loaded = JsonSerializer.Deserialize<AssistSettings>(File.ReadAllText(FilePath));
                    if (loaded != null) { loaded.Clamp(); return loaded; }
                }
            }
            catch { }
            return new AssistSettings();
        }

        public static void Save(AssistSettings settings)
        {
            Directory.CreateDirectory(Folder);
            settings.Clamp();
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
