using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionsAspireApril
{
    public class MyBlobTrigger
    {
        private readonly ILogger<MyBlobTrigger> _logger;

        public MyBlobTrigger(ILogger<MyBlobTrigger> logger)
        {
            _logger = logger;
        }

        [Function(nameof(MyBlobTrigger))]
        public async Task Run([BlobTrigger("samples-workitems/{name}", Connection = "MyBlobConnection")] Stream stream, string name)
        {
            using var blobStreamReader = new StreamReader(stream);
            var content = await blobStreamReader.ReadToEndAsync();
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");
        }
    }
}
