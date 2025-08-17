using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Tsg.Rdc.Model.SymX;

namespace Tsg.Rdc.Triggers;

public class RdcSymxResponseHandler
{
    private readonly ILogger<RdcSymxResponseHandler> _logger;
    private readonly string _storageConn;
    private const string TableName = "depositTransaction";
    private const string PartitionKey = "deposit";


    public RdcSymxResponseHandler(ILogger<RdcSymxResponseHandler> logger)
    {
        _logger = logger;
        _storageConn = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                       ?? throw new InvalidOperationException("AzureWebJobsStorage is not set.");
    }

    [Function(nameof(RdcSymxResponseHandler))]
    public async Task Run([QueueTrigger("deposit-callback", Connection = "AzureWebJobsStorage")] QueueMessage message)
    {
        _logger.LogInformation("RdcSymxResponseHandler processed:\n{MessageText}", message.MessageText);

        var response = JsonSerializer.Deserialize<SymXCallResponse>(message.MessageText);
        
        if (response == null)
        {
            _logger.LogError("Failed to deserialize SymXCallResponse from message: {MessageText}", message.MessageText);
            return;
        }
        
        var table  = new TableClient(_storageConn, TableName);
        TableEntity entity;
        try
        {
            var resp = table.GetEntity<TableEntity>(PartitionKey, response.CorrelationId);
            entity = resp.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogError(ex, "No table entity found for PK={PK} RK={RK}", PartitionKey, response.CorrelationId);
            return;
        }
        
        // Update Table entity with response/status
        if (!entity.TryGetValue("Attempts", out var attemptsObj) || attemptsObj is not int attempts)
        {
            attempts = 1;
        }
        else
        {
            attempts += 1;
        }
        entity["Attempts"] = attempts;
        entity["Status"] = response.Status;
        entity["LastUpdatedUtc"] = DateTime.UtcNow;
        entity["CallCompleteUtc"] = response.Timestamp;
        entity["CallResponse"] = message.MessageText;
        await table.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);
        
        if (response.Status == "error")
        {
            var retryQueue = new QueueClient(_storageConn, "deposit-inbound", new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            await retryQueue.CreateIfNotExistsAsync();
            await retryQueue.SendMessageAsync(response.CorrelationId);
            _logger.LogInformation("Enqueued retry for CorrelationId={CorrelationId} due to error status in SymX response.", response.CorrelationId);
        }
        
    }
}