using Azure.Storage.Blobs;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EscortsReady.Utilities
{
    internal class Storage
    {
        private static BlobServiceClient? client;
        private static BlobContainerClient? container;

        public static async Task Setup(string? storageConnectionString)
        {
            client = new BlobServiceClient(storageConnectionString);
            var containerName = $"escortready";
            container = client.GetBlobContainerClient(containerName);
            if (container == null)
                container = await client.CreateBlobContainerAsync(containerName);
            Console.WriteLine($"Azure Storage Ready!");
        }
        public static async Task UploadFileAsync(string filename, DiscordGuild? guild = null)
        {
            var info = new FileInfo(filename);
            var root = guild != null ? Path.Combine("EscortReady", $"{guild.Name}_{guild.Id}", info.Name) : Path.Combine("EscortReady", "Resources",  info.Name);
            filename = string.Join("", filename.ToCharArray().Where(x => x < 127));
            // Convert object;
            var blobClient = container.GetBlobClient(root);
            var metadata = new Dictionary<string, string>();
            metadata.Add("Name", info.Name);
            metadata.Add("DateCrated", DateTime.Now.ToString());
            await blobClient.UploadAsync(path: filename, metadata: metadata);
            blobClient.SetMetadata(metadata);
        }        
        public static string? GetFileUrl(string filename, DiscordGuild? guild = null)
        {
            var info = new FileInfo(filename);
            var root = guild != null ? Path.Combine("EscortReady", $"{guild.Name}_{guild.Id}", info.Name) : Path.Combine("EscortReady", "Resources", info.Name);
            filename = string.Join("", filename.ToCharArray().Where(x => x < 127));
            var blobs = container.GetBlobs().ToList();
            var file = blobs.Count > 0 ? blobs.Find(x => string.Equals(x.Name, root.Replace("\\", "/"), StringComparison.OrdinalIgnoreCase)) : null;
            if (file == null) return GetFileUrl("EscortsReady.png");
            var blobClient = container.GetBlobClient(file.Name);
            return blobClient.Uri.ToString();

        }
        public static Stream? GetFileStream(string filename, DiscordGuild? guild = null)
        {
            var info = new FileInfo(filename);
            var root = guild != null ? Path.Combine("EscortReady", $"{guild.Name}_{guild.Id}", info.Name) : Path.Combine("EscortReady", "Resources", info.Name);
            filename = string.Join("", filename.ToCharArray().Where(x => x < 127));
            var blobs = container.GetBlobs().ToList();
            var file = blobs.Count > 0 ? blobs.Find(x => string.Equals(x.Name, root.Replace("\\", "/"), StringComparison.OrdinalIgnoreCase)) : null;
            if (file != null)
            {
                var blobClient = container.GetBlobClient(file.Name);
                var ms = new MemoryStream();
                blobClient.Download().Value.Content.CopyTo(ms);
                ms.Position = 0;
                return ms;
            }
            return null;
        }
    }
}