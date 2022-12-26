using Newtonsoft.Json;
using System.Reflection;

namespace EscortsReady
{
    public abstract class FileData
    {
        [JsonIgnore] public static string rootdir => $"{new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName}\\Resources\\Data";

        public virtual async Task CloseAsync() { GC.SuppressFinalize(this); await Task.CompletedTask; }
        public virtual string ToJson()
        {
            var json = string.Empty;
            json = JsonConvert.SerializeObject(this, Utils.serializerSettings);
            return json;
        }
    }
}