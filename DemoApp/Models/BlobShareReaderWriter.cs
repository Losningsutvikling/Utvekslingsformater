using Azure.Identity;
using Azure.Storage.Blobs;
using System.Xml;

namespace DemoApp.Models
{
    public class BlobShareReaderWriter
    {
        public BlobServiceClient blobServiceClient { get; set; }
        public static BlobContainerClient GetBlobStorageClient(IConfiguration config)
        {
            BlobContainerClient blobClient;
            // Er det fra lokal maskin hvor man må bruke en connection string?
            if (!string.IsNullOrEmpty(config["StorageAccountConnectString"]))
            {
                blobClient = new(config["StorageAccountConnectString"], config["StorageContainerUri"]);
                return blobClient;
            }
            /*Bruker System assigned managed identity med tildelte rettigheter 'Storage Blob Data Contributor'*/
            //Hvis det er aktuelt å bruke 'User assigned managed identities' brukes ["StorageAccountClientId"]:
            string clientId = config["StorageAccountClientId"];
            DefaultAzureCredential credential;
            if (!string.IsNullOrEmpty(clientId))
            {
                credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = clientId });
            }
            else
            {
                credential = new DefaultAzureCredential();
            }
            blobClient = new BlobContainerClient(new Uri(config["StorageContainerUri"]), credential);

            return blobClient;
        }

        public static string TestConnection(IConfiguration config)
        {
            try
            {
                var client = GetBlobStorageClient(config);
                return "OK "; //+ connectString;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static async Task SaveFileContents(IConfiguration config, string fileName, string contents)
        {
            var client = GetBlobStorageClient(config);
            var blobClient = client.GetBlobClient(fileName);
            MemoryStream stream = new() { Position = 0 };
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(contents);
            writer.Flush();
            stream.Position = 0;
            var _ = blobClient.Upload(stream);
            //var val = result.Value;
        }

        public static List<string> GetAllFiles(IConfiguration config, string folder)
        {
            try
            {
                List<string> files = new();
                var client = GetBlobStorageClient(config);
                var listing = client.GetBlobs();
                foreach (var item in listing.Where(i => i.Name.StartsWith(folder)))
                {
                    files.Add(item.Name);
                }
                return files;
            }
            catch (Exception e)
            {
                throw new Exception($"{e.Message}\n\n{e.StackTrace}");
            }
        }


        public static async Task<MemoryStream> GetFileStream(IConfiguration config, string name)
        {
            var client = GetBlobStorageClient(config);
            var blobClient = client.GetBlobClient(name);
            MemoryStream stream = new();
            var downloadResponse = blobClient.DownloadTo(stream);
            if (downloadResponse.Status > 300)
                throw new Exception($"Feil i download av fil {name}: HTTP {downloadResponse}");
            stream.Position = 0;
            return await Task.FromResult(stream);
        }

        public static async Task<string> GetFileContents(IConfiguration config, string name)
        {
            var client = GetBlobStorageClient(config);
            var blobClient = client.GetBlobClient(name);
            MemoryStream stream = new();
            var downloadResponse = blobClient.DownloadTo(stream);
            if (downloadResponse.Status > 300)
                throw new Exception($"Feil i download av fil {name}: HTTP {downloadResponse}");
            stream.Position = 0;
            var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            return content;
        }
    }
}
