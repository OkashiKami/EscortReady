using DSharpPlus.Entities;
using EscortsReady;
using EscortsReady.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EscortReady
{
    public class AssetDatabase : FileData
    {

        public static async Task<T> LoadAsync<T>(DiscordGuild guild)
        {
            try
            {
                if (typeof(T).Equals(typeof(Settings)))
                {
                    var file = $"{rootdir}\\{guild.Name}_{guild.Id}\\Settings.json";
                    Settings tempObject = null;
                    using (var stream = Storage.GetFileStream(file, guild))
                    {
                        if (stream == null)
                        {
                            tempObject = new Settings();
                            return (T)(object)tempObject;
                        }
                        using (var sr = new StreamReader(stream, Encoding.Unicode))
                        {
                            try
                            {
                                var json = await sr.ReadToEndAsync();
                                tempObject = JsonConvert.DeserializeObject<Settings>(json, Utils.serializerSettings);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                tempObject = null;
                            }
                        }
                    }
                    return (T)(object)tempObject;
                }
                if (typeof(T).Equals(typeof(Escorts)))
                {
                    var file = $"{rootdir}\\{guild.Name}_{guild.Id}\\Profiles.json";
                    Escorts tempObject = null;
                    using (var stream = Storage.GetFileStream(file, guild))
                    {
                        if (stream == null)
                        {
                            tempObject = new Escorts();
                            return (T)(object)tempObject;
                        }
                        using (var sr = new StreamReader(stream, Encoding.Unicode))
                        {
                            try
                            {
                                var json = await sr.ReadToEndAsync();
                                tempObject = JsonConvert.DeserializeObject<Escorts>(json, Utils.serializerSettings);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                tempObject = null;
                            }
                        }
                    }
                    return (T)(object)tempObject;
                }
                if (typeof(T).Equals(typeof(Members)))
                {
                    var file = $"{rootdir}\\{guild.Name}_{guild.Id}\\Members.json";
                    Members tempObject = null;
                    using (var stream = Storage.GetFileStream(file, guild))
                    {
                        if (stream == null)
                        {
                            tempObject = new Members();
                            return (T)(object)tempObject;
                        }
                        using (var sr = new StreamReader(stream, Encoding.Unicode))
                        {
                            try
                            {
                                var json = await sr.ReadToEndAsync();
                                tempObject = JsonConvert.DeserializeObject<Members>(json, Utils.serializerSettings);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                tempObject = null;
                            }
                        }
                    }
                    return (T)(object)tempObject;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Utils.ReportException(ex);
            }
            return default;
        }
        public static async Task SaveAsync<T>(DiscordGuild guild, T value)
        {
            try
            {
                if (typeof(T).Equals(typeof(Settings)))
                {
                    var file = $"{rootdir}\\{guild.Name}_{guild.Id}\\Settings.json";
                    Directory.CreateDirectory(new FileInfo(file).Directory.FullName);
                    using (var fs = File.Open(file, FileMode.Create))
                    {
                        using (var sw = new StreamWriter(fs, Encoding.Unicode))
                        {
                            var json = JsonConvert.SerializeObject(value, Utils.serializerSettings);
                            await sw.WriteAsync(json);
                        }
                        await Storage.UploadFileAsync(file, guild);
                    }
                    Directory.CreateDirectory(new FileInfo(file).Directory.FullName);
                }
                if (typeof(T).Equals(typeof(Escorts)))
                {
                    var file = $"{rootdir}\\{guild.Name}_{guild.Id}\\Profiles.json";
                    Directory.CreateDirectory(new FileInfo(file).Directory.FullName);
                    using (var fs = File.Open(file, FileMode.Create))
                    {
                        using (var sw = new StreamWriter(fs, Encoding.Unicode))
                        {
                            var json = JsonConvert.SerializeObject(value, Utils.serializerSettings);
                            await sw.WriteAsync(json);
                        }
                        await Storage.UploadFileAsync(file, guild);
                    }
                    Directory.CreateDirectory(new FileInfo(file).Directory.FullName);
                }
                if (typeof(T).Equals(typeof(Members)))
                {
                    var file = $"{rootdir}\\{guild.Name}_{guild.Id}\\Members.json";
                    Directory.CreateDirectory(new FileInfo(file).Directory.FullName);
                    using (var fs = File.Open(file, FileMode.Create))
                    {
                        using (var sw = new StreamWriter(fs, Encoding.Unicode))
                        {
                            var json = JsonConvert.SerializeObject(value, Utils.serializerSettings);
                            await sw.WriteAsync(json);
                        }
                        await Storage.UploadFileAsync(file, guild);
                    }
                    Directory.CreateDirectory(new FileInfo(file).Directory.FullName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Utils.ReportException(ex);
            }
        }
    }

    public class Settings : FileData
    {
        public Dictionary<string, object> library = new Dictionary<string, object>();        
        public void Remove(string key) => library.Remove(key);
        public bool HasKey(string key) => library.ContainsKey(key);
        public void Clear() => library = new Dictionary<string, object>();

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
    public class Escorts : FileData
    {
        public Dictionary<ulong, Profile> library = new Dictionary<ulong, Profile>();
        public void Remove(ulong key) => library.Remove(key);
        public bool HasKey(ulong key) => library.ContainsKey(key);
        public Profile Find(Predicate<Profile> predict) => library.Values.ToList().Find(predict);
        public void Clear() => library = new Dictionary<ulong, Profile>();

        public Profile? this[ulong key]
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
    public class Members : FileData
    {
        public Dictionary<ulong, Member> library = new Dictionary<ulong, Member>();
        public void Remove(ulong key) => library.Remove(key);
        public bool HasKey(ulong key) => library.ContainsKey(key);
        public Member Find(Predicate<Member> predict) => library.Values.ToList().Find(predict);
        public void Clear() => library = new Dictionary<ulong, Member>();

        public Member? this[ulong key]
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