using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ObjectDetection.src.EventHub;
using ObjectDetection.src.StorageManager;

namespace ObjectDetection.src.API
{
    [Route("api")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        BlobInformation blobInformation = new BlobInformation
        {
            containerName = "depression-detection-video",
            connectionString = BlobService.connectionString
        };

        [HttpPost("customer")]
        public async Task<IActionResult> PostCustomer([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            return BadRequest("No file sended.");
            
            string json = @"
            {
              ""id_user"": ""12345"",
              ""id_video"": ""abcde6789"",
              ""options"": {
                ""objectsSelected"": [""car"", ""person"", ""bicycle""]
              }
            }";
            var _object = JsonDocument.Parse(json);
            _object.RootElement.TryGetProperty("id_user", out JsonElement _idUser);
            _object.RootElement.TryGetProperty("id_user", out JsonElement _idVideo);
            blobInformation.blobName = $"{_idUser}-{_idVideo}";
            EventHubClient.SendMessage(json);
            using (var stream = file.OpenReadStream())
            {
                await BlobService.UploadFileToBlobAsync(blobInformation, stream);
            }
            return Ok("Started");
        }

    }
}
