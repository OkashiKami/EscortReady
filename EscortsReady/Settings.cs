using DSharpPlus.Entities;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace EscortsReady
{
    public class Settings : FileData
    {
        public Dictionary<string, object> library = new Dictionary<string, object>();
        public static async Task<Settings> LoadAsync(DiscordGuild guild)
        {
            var file = $"{rootdir}\\{guild.Name}_{guild.Id}\\Settings.json";
            Directory.CreateDirectory(new FileInfo(file).Directory.FullName);
            Settings settingsObject = null;
            if(!File.Exists(file))
            {
                settingsObject  = new Settings();
                return settingsObject;
            }
            using (var fs = File.OpenRead(file))
            {
                using (var sr = new StreamReader(fs, Encoding.Unicode))
                {
                    try
                    {
                        var json = await sr.ReadToEndAsync();
                        settingsObject = JsonConvert.DeserializeObject<Settings>(json, Utils.serializerSettings);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        settingsObject = null;
                    }
                }
            }
            return settingsObject;
        }
        public static async Task SaveAsync(DiscordGuild guild, Settings settings)
        {
            var file = $"{rootdir}\\{guild.Name}_{guild.Id}\\Settings.json";
            Directory.CreateDirectory(new FileInfo(file).Directory.FullName);

            using (var fs = File.Open(file, FileMode.Create))
            {
                using (var sw = new StreamWriter(fs, Encoding.Unicode))
                {
                    var json = JsonConvert.SerializeObject(settings, Utils.serializerSettings);
                    await sw.WriteAsync(json);
                }
            }
        }
        public object? this[string key]
        {
            get
            {
                var hasKey = library.ContainsKey(key);
                if (hasKey)
                    return library[key];
                else return null;
            }
            set
            {
                var hasKey = library.ContainsKey(key);
                if (!hasKey && value != null)
                    library.Add(key, value);
                else if (hasKey && value != null)
                    library[key] = value;
            }
        }
    }
}