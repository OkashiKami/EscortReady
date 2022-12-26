using Azure.Storage.Blobs;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System.Reflection;

namespace EscortsReady
{
    internal class Storage
    {
        private static BlobServiceClient client;
        private static BlobContainerClient container;

        public static async Task Setup(string? storageConnectionString)
        {
            client = new BlobServiceClient(storageConnectionString);
            var containerName = $"escortreadyfiles";
            container = client.GetBlobContainerClient(containerName);
            if(container == null)
                container = await client.CreateBlobContainerAsync(containerName);
            Console.WriteLine($"Azure Storage Ready!");
        }
        public static object? GetFile(DiscordGuild guild, string name, StorageFileType type)
        {
            var blobs = container.GetBlobs().ToList();
            var file = blobs.Count > 0 ?  
                blobs.Find(x => x.Name.Contains(name) && x.Name.Contains(type.ToString())) : null;



            if(file == null)
                UploadFile(name, type, guild);
            var blobClient = container.GetBlobClient(file.Name);                
            var json = string.Empty;
            using (var sr = new StreamReader(blobClient.Download().Value.Content, true))
                json = sr.ReadToEnd();
            switch (type)
            {
                case StorageFileType.Settings:
                    var gsettings = JsonConvert.DeserializeObject<DiscordGuild>(json, Utils.serializerSettings);
                    return gsettings;
            }
            return file;
        }
        public static void UploadFile(string name, StorageFileType type, object value)
        {
            // Convert object;
            var fileName = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, $"tmp_{name}_{type}.json");
            var blobClient = container.GetBlobClient(new FileInfo(fileName).Name);
            var metadata = new Dictionary<string, string>();

            switch (type)
            {
                case StorageFileType.Settings:
                    var json = JsonConvert.SerializeObject(value, Utils.serializerSettings);
                    File.WriteAllText(fileName, json);
                    break;
            }


            metadata.Add("Name", name);
            metadata.Add("StorageFileType", type.ToString());
            metadata.Add("DateCrated", DateTime.Now.ToString());
            blobClient.Upload(path: fileName, metadata: metadata);
            blobClient.SetMetadata(metadata);
            File.Delete(fileName);
        }
    }
    
}