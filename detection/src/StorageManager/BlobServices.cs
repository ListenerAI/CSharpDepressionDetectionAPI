using System.Text.Json;
using Azure.Storage.Blobs;

namespace ObjectDetection.src.StorageManager
{
    public class BlobService
    {
        public static readonly string
            connectionString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING") ?? string.Empty;

        /// <summary>
        /// Gets a specific property from a JSON blob.
        /// </summary>
        public static async Task<string> GetDataFromBlob(BlobInformation info, string propertyName)
        {
            try
            {
                // Remove the 'using' statement for BlobClient
                var blobClient = new BlobClient(info.connectionString, info.containerName, info.blobName);

                // Use 'using' for the stream and reader
                using var blobStream = await blobClient.OpenReadAsync();
                using var reader = new StreamReader(blobStream);
                string jsonContent = await reader.ReadToEndAsync();

                // Use 'using' for JsonDocument
                using JsonDocument jsonDocument = JsonDocument.Parse(jsonContent);
                if (jsonDocument.RootElement.TryGetProperty(propertyName, out JsonElement property))
                {
                    return property.ToString();
                }

                return "Error";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting data from blob: {ex.Message}");
                return "Error";
            }
        }
        /// <summary>
        /// Gets the entire JSON content from a blob.
        /// </summary>
        public static async Task<string> GetJsonFromBlob(BlobInformation info)
        {
            try
            {
                var blobClient = new BlobClient(info.connectionString, info.containerName, info.blobName);
                var response = await blobClient.DownloadContentAsync();  // Alternativa a OpenReadAsync
                return response.Value.Content.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting JSON from blob {info.blobName}, container {info.containerName}: {ex.Message}");
                return "Error";
            }
        }


        /// <summary>
        /// Uploads JSON content to a blob.
        /// </summary>
        public static async Task UploadFileToBlobAsync(BlobInformation info, Stream fileStream)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(info.connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(info.containerName);

                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient(info.blobName);

                await blobClient.UploadAsync(fileStream, overwrite: true);

                Console.WriteLine($"{info.blobName} uploaded to {info.containerName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file to blob: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a blob if it exists.
        /// </summary>
        public static async Task DeleteJsonAsync(BlobInformation info)
        {
            try
            {
                var containerClient = new BlobContainerClient(info.connectionString, info.containerName);
                var blobClient = containerClient.GetBlobClient(info.blobName);

                await blobClient.DeleteIfExistsAsync();
                Console.WriteLine($"{info.blobName} deleted from {info.containerName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting blob: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method to create a BlobClient.
        /// </summary>
        private static BlobClient CreateBlobClient(BlobInformation info)
        {
            return new BlobClient(info.connectionString, info.containerName, info.blobName);
        }
    }
}