using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using FuncTest.Model.Ensenta;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace FuncTest;

public class DepositTransactionTester
{
    private readonly ILogger<DepositTransaction> _logger;

    public DepositTransactionTester(ILogger<DepositTransaction> logger)
    {
        _logger = logger;
    }

    [Function("testDepositTransaction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        
        string xml;
        using (var reader = new StreamReader(req.Body, Encoding.UTF8))
        {
            xml = await reader.ReadToEndAsync();
        }
        
        _logger.LogInformation(xml);
        
        // Deserialize XML to SoapEnvelope
        var serializer = new XmlSerializer(typeof(EnsentaSoapEnvelope));
        EnsentaSoapEnvelope envelope;
        using (var stringReader = new StringReader(xml))
        {
            envelope = (EnsentaSoapEnvelope)serializer.Deserialize(stringReader);
        }
        
        // Get the DoDepositTransaction object
        var depositTransaction = envelope.Body.DoDepositTransaction;
        
        // Serialize object to JSON
        var json = JsonSerializer.Serialize(depositTransaction, new JsonSerializerOptions { WriteIndented = true });
        
        // Return JSON result
        return new ContentResult
        {
            Content = json,
            ContentType = "application/json",
            StatusCode = 200
        };
    }
}
