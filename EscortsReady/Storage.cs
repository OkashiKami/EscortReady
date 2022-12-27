using Azure.Storage.Blobs;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace EscortsReady
{
    internal class Storage
    {
        private static BlobServiceClient client;
        private static BlobContainerClient container;

        public static async Task Setup(string? storageConnectionString)
        {
            client = new BlobServiceClient(storageConnectionString);
            var containerName = $"escortready";
            container = client.GetBlobContainerClient(containerName);
            if(container == null)
                container = await client.CreateBlobContainerAsync(containerName);
            Console.WriteLine($"Azure Storage Ready!");
        }
        public static string? GetFileUrl(string filename)
        {
            filename = string.Join("", filename.ToCharArray().Where(x => ((int)x) < 127));
            var blobs = container.GetBlobs().ToList();
            var file = blobs.Count > 0 ? blobs.Find(x => x.Name.Contains(filename)) : null;
            if (file == null) return GetFileUrl("EscortsReady.png");
            var blobClient = container.GetBlobClient(file.Name);
            return blobClient.Uri.ToString();
            
        }
        public static Stream? GetFileStream(string filename)
        {
            filename = string.Join("", filename.ToCharArray().Where(x => ((int)x) < 127));
            var blobs = container.GetBlobs().ToList();
            var file = blobs.Count > 0 ?
                blobs.Find(x => x.Name.Contains(filename)) : null;
            var blobClient = container.GetBlobClient(file.Name);
            return blobClient.Download().Value.Content;

        }


        public static async Task UploadFile(string filename)
        {
            filename = string.Join("", filename.ToCharArray().Where(x => ((int)x) < 127));
            // Convert object;
            var fi = new FileInfo(filename);
            var blobClient = container.GetBlobClient(fi.Name);
            var metadata = new Dictionary<string, string>();
            metadata.Add("Name", fi.Name);
            metadata.Add("DateCrated", DateTime.Now.ToString());
            blobClient.Upload(path: filename, metadata: metadata);
            blobClient.SetMetadata(metadata);
        }
    }
    
}