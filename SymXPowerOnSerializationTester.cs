using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using FuncTest.Model.SymX;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FuncTest;

public class SymXPowerOnSerializationTester
{
    private readonly ILogger<SymXPowerOnSerializationTester> _logger;

    public SymXPowerOnSerializationTester(ILogger<SymXPowerOnSerializationTester> logger)
    {
        _logger = logger;
    }

    [Function("SymXXmlToJson")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function,"post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        
        string xml;
        using (var reader = new StreamReader(req.Body, Encoding.UTF8))
        {
            xml = await reader.ReadToEndAsync();
        }
        
        _logger.LogInformation(xml);
        
        // Deserialize XML to SoapEnvelope
        var serializer = new XmlSerializer(typeof(SymXSoapEnvelope));
        SymXSoapEnvelope envelope;
        using (var stringReader = new StringReader(xml))
        {
            envelope = (SymXSoapEnvelope)serializer.Deserialize(stringReader);
        }
        
        // Serialize object to JSON
        var json = JsonSerializer.Serialize(envelope, new JsonSerializerOptions { WriteIndented = true });
        
        // Return JSON result
        return new ContentResult
        {
            Content = json,
            ContentType = "application/json",
            StatusCode = 200
        };
    }
}
