using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure.Data.Tables;
using Azure.Storage.Queues;

namespace FuncTest;

public class DepositTransaction
{
    private readonly ILogger<DepositTransaction> _logger;

    public DepositTransaction(ILogger<DepositTransaction> logger)
    {
        _logger = logger;
    }

    [Function("DepositTransaction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        Guid transactionId = Guid.NewGuid();
        _logger.LogInformation("DoDepositTransaction received a request. {TransactionId}", transactionId);

        // Read raw SOAP/XML
        string xml;
        using (var reader = new StreamReader(req.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
        {
            xml = await reader.ReadToEndAsync();
        }
        _logger.LogInformation("XML length: {Len}", xml?.Length ?? 0);

        // Storage connection (Azurite)
        var storageConn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        // --- Write to Table: depositTransaction ---
        var tableClient = new TableClient(storageConn, "depositTransaction");
        await tableClient.CreateIfNotExistsAsync();

        // Use a simple partition; transactionId is the RowKey ("primary key")
        var entity = new TableEntity(partitionKey: "deposit", rowKey: transactionId.ToString())
        {
            { "ReceivedUtc", DateTime.UtcNow },
            { "Status", "received" },
            { "LastUpdatedUtc", DateTime.UtcNow },
            { "Attempts", 0 },
            { "Xml", xml }   // If payloads can be huge, store in Blob instead and save a pointer here
        };
        await tableClient.AddEntityAsync(entity);

        // --- Send to Queue: deposit-inbound ---
        var queueClient = new QueueClient(storageConn, "deposit-inbound", new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        });
        await queueClient.CreateIfNotExistsAsync();
        await queueClient.SendMessageAsync(transactionId.ToString());

        return new AcceptedResult(string.Empty, new
        {
            status = "accepted",
            message = "SOAP payload received and queued for processing",
            transactionId = transactionId
        });
    }
}
