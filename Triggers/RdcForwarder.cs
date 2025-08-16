using System.Text.Json;
using System.Xml.Serialization;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Tsg.Rdc.Model.Ensenta;
using Tsg.Rdc.Model.RDCSystem;

namespace Tsg.Rdc.Triggers;

public class RdcForwarder
{
    private readonly ILogger<RdcForwarder> _logger;
    private readonly string _storageConn;
    private const string TableName = "depositTransaction";
    private const string PartitionKey = "deposit";


    public RdcForwarder(ILogger<RdcForwarder> logger)
    {
        _logger = logger;
        _storageConn = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                       ?? throw new InvalidOperationException("AzureWebJobsStorage is not set.");
    }

    [Function(nameof(RdcForwarder))]
    public async Task Run([QueueTrigger("deposit-inbound", Connection = "AzureWebJobsStorage")] QueueMessage message)
    {
        string transactionId = message.MessageText;

        // 1) Fetch XML from Azure Table Storage
        var table = new TableClient(_storageConn, TableName);

        TableEntity entity;
        try
        {
            var resp = table.GetEntity<TableEntity>(PartitionKey, transactionId);
            entity = resp.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogError(ex, "No table entity found for PK={PK} RK={RK}", PartitionKey, transactionId);
            return;
        }

        if (!entity.TryGetValue("Xml", out var raw) || raw is not string xml || string.IsNullOrWhiteSpace(xml))
        {
            _logger.LogError("Entity found but 'Xml' property is missing/empty. transactionId={TransactionId}", transactionId);
            return;
        }
        
        // Deserialize XML to SoapEnvelope
        var serializer = new XmlSerializer(typeof(EnsentaSoapEnvelope));
        EnsentaSoapEnvelope envelope;
        using (var stringReader = new StringReader(xml))
        {
            envelope = (EnsentaSoapEnvelope)serializer.Deserialize(stringReader);
        }
        
        // Get the DoDepositTransaction object
        var depositTransaction = envelope.Body.DoDepositTransaction;

        _logger.LogInformation("Deposit transaction ID {TransactionId} corresponds to {TransactionNumber}",
            transactionId, depositTransaction.receiptTransactionNumber);
        RdcCallParams rg = new RdcCallParams
        {
            TransactionId = transactionId,
            ReceiptTransactionNumber = depositTransaction.receiptTransactionNumber,
            DepositItemCount = depositTransaction.depositItems.Count,
            CodeLine = null,
            HostHoldCode = null,
            Amount = 0m
        };
        foreach (var di in depositTransaction.depositItems)
        {
            _logger.LogInformation(" - Item: HoldCode={Code}, CodeLine={CodeLine}, Amount={Amount}",
                di.HostHoldCode, di.CodeLine, di.Amount);
            if (decimal.TryParse(di.Amount, out var amt)) rg.Amount += amt;
            if (rg.HostHoldCode == null) rg.HostHoldCode = di.HostHoldCode;
            if (rg.CodeLine == null) rg.CodeLine = di.CodeLine;
        }
        string rgUserNum1 = (rg.Amount * 100).ToString("F0");
        _logger.LogInformation(" - Total deposit amount: {Total:C2}, RGUserNum1={RgUserNum1}, Item Count = {ItemCount}", 
            rg.Amount, rgUserNum1, depositTransaction.depositItems.Count);
        
        var json = JsonSerializer.Serialize(rg, new JsonSerializerOptions { WriteIndented = true });
        
        // --- Send to Queue: deposit-inbound ---
        var queueClient = new QueueClient(_storageConn, "deposit-forward", new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        });
        await queueClient.CreateIfNotExistsAsync();
        await queueClient.SendMessageAsync(json);

        // Update status and last updated timestamp
        entity["Status"] = "forwarded";
        entity["LastUpdatedUtc"] = DateTime.UtcNow;
        await table.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);

        await Task.CompletedTask;
    }
}