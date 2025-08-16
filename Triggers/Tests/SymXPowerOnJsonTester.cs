using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using FuncTest.Model.SymX;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FuncTest.Triggers.Tests;

public class SymXPowerOnJsonTester
{
    private readonly ILogger<SymXPowerOnJsonTester> _logger;

    public SymXPowerOnJsonTester(ILogger<SymXPowerOnJsonTester> logger)
    {
        _logger = logger;
    }

    [Function("testSymXJsonToXml")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function,"post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        
        string json;
        using (var reader = new StreamReader(req.Body, Encoding.UTF8))
        {
            json = await reader.ReadToEndAsync();
        }
        
        _logger.LogInformation(json);
        
        var symxPowerOn = JsonSerializer.Deserialize<SymXSoapEnvelope>(json);
        
// Serialize object to XML as UTF-8
        var serializer = new XmlSerializer(typeof(SymXSoapEnvelope));
        string xml;
        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false), // false = no BOM
            Indent = true
        };
        using (var stream = new MemoryStream())
        using (var writer = XmlWriter.Create(stream, settings))
        {
            serializer.Serialize(writer, symxPowerOn);
            xml = Encoding.UTF8.GetString(stream.ToArray());
        }
        // Return JSON result
        return new ContentResult
        {
            Content = xml,
            ContentType = "application/xml",
            StatusCode = 200
        };
    }
}
