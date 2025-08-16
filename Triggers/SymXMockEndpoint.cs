using System.Text;
using System.Xml.Serialization;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Tsg.Rdc.Model.SymX;

namespace Tsg.Rdc.Triggers;

public class SymXMockEndpoint
{
    private readonly ILogger<SymXMockEndpoint> _logger;

    public SymXMockEndpoint(ILogger<SymXMockEndpoint> logger)
    {
        _logger = logger;
    }

    [Function("SymXMockEndpoint")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        // Grab a timestamp for when the request was received.
        var timestamp = DateTime.UtcNow;
        
        // Grab the Symx Call ID from the request headers if present.
        var symxCallId = req.Headers.ContainsKey("x-SymX-Call-ID") ? req.Headers["x-SymX-Call-ID"].ToString() : null;
        
        // Grab the correlation ID from the request headers if present.
        var correlationId = req.Headers.ContainsKey("x-Correlation-ID") ? req.Headers["x-Correlation-ID"].ToString() : null;
        
        // Read the request body and log it for debugging.
        string xml;
        using (var reader = new StreamReader(req.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
        {
            xml = await reader.ReadToEndAsync();
        }
        _logger.LogInformation("SymXMockEndpoint received a request at {Timestamp} with SymX-Call-ID: {SymXCallId} and Correlation-ID: {CorrelationId}. XML length: {Len}", 
            timestamp, symxCallId ?? "N/A", correlationId ?? "N/A", xml?.Length ?? 0);
        
        // Parse the request and validate its structure.
        var serializer = new XmlSerializer(typeof(SymXSoapEnvelope));
        SymXSoapEnvelope envelope;
        using (var stringReader = new StringReader(xml))
        {
            envelope = (SymXSoapEnvelope)serializer.Deserialize(stringReader);
        }

        if (envelope == null)
        {
            _logger.LogError("Failed to deserialize the incoming SymX SOAP envelope for SymX-Call-ID: {SymxCallId}.", symxCallId);
            return new BadRequestObjectResult("Invalid SOAP envelope for SymX-Call-ID: " + symxCallId);
        }

        // Store the request in an Azure Table including the timestamp it was received.
        if (!string.IsNullOrWhiteSpace(symxCallId))
        {
            await StoreRequestAsync(symxCallId, correlationId, timestamp, xml);
        }
        else
        {
            _logger.LogWarning("x-SymX-Call-ID header is missing; skipping request storage.");
        }
        
        // Return a mock response that mimics the real SymX service.
        return new AcceptedResult(string.Empty, new
        {
            status = "accepted",
            timestamp = timestamp,
            symxCallId = symxCallId,
            correlationId = correlationId,
            totalAmount = envelope.Body?.ExecutePowerOnReturnArray.Request.Body.UserDefinedParameters.RGUserNum[0].Value,
            message = "SOAP payload applied to Symitar",
        });

    }
    
    private async Task StoreRequestAsync(string symxCallId, string correlationId, DateTime timestamp, string xml)
    {
        var storageConn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        var tableClient = new TableClient(storageConn, "syntheticSymXRequests");
        await tableClient.CreateIfNotExistsAsync();

        var entity = new TableEntity(partitionKey: "deposit", rowKey: symxCallId)
        {
            { "ReceivedUtc", timestamp },
            { "CorrelationId", correlationId ?? string.Empty },
            { "LastUpdatedUtc", DateTime.UtcNow },
            { "Xml", xml }   // If payloads can be huge, store in Blob instead and save a pointer here
        };
        await tableClient.AddEntityAsync(entity);
    }
}
