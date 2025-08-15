using System;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using FuncTest.Model.RDCSystem;
using FuncTest.Model.SymX;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FuncTest;

public class RdcSymxCallBuilder
{
    private readonly ILogger<RdcSymxCallBuilder> _logger;
    private readonly string _storageConn;
    // private const string TableName = "depositTransaction";
    // private const string PartitionKey = "deposit";


    public RdcSymxCallBuilder(ILogger<RdcSymxCallBuilder> logger)
    {
        _logger = logger;
        _storageConn = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                       ?? throw new InvalidOperationException("AzureWebJobsStorage is not set.");
    }

    [Function(nameof(RdcSymxCallBuilder))]
    public async Task Run([QueueTrigger("deposit-forward", Connection = "AzureWebJobsStorage")] QueueMessage message)
    {
        _logger.LogInformation(message.MessageText);
        
        var rdcCallParams = JsonSerializer.Deserialize<RDCForwarder>(message.MessageText);

        var rdcCall = CreateSymXCall(rdcCallParams);
        
        var symXCall = new SymXCall
        {
            CorrelationId = rdcCallParams.TransactionId,
            CallbackQueue = "deposit-callback",
            SymXInstance = "RDC",
            SymXEnvelope = rdcCall
        };
        
        var json = JsonSerializer.Serialize(symXCall, new JsonSerializerOptions { WriteIndented = true });
        
        var queueClient = new QueueClient(_storageConn, "symx-outbound", new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        });
        await queueClient.CreateIfNotExistsAsync();
        await queueClient.SendMessageAsync(json);
        
        _logger.LogInformation("RDC SymX Call XML for Transaction ID {TransactionId}:\n{Xml}", rdcCallParams.TransactionId, json);
        
    }
    
    private SymXSoapEnvelope CreateSymXCall(RDCForwarder rdcCallParams)
    {
        return new SymXSoapEnvelope
        {
            Header = new SymXSoapHeader
            {
            },
            Body = new SymXSoapBody
            {
                ExecutePowerOnReturnArray = new ExecutePowerOnReturnArray
                {
                    Request = new Request
                    {
                        BranchId = 0,
                        Credentials = new Credentials
                        {
                            ProcessorUser = "821",
                            AdministrativeCredentials = new AdministrativeCredentials()
                            {
                                Password = "PASSWORD"
                            }
                        },
                        DeviceInformation = new DeviceInformation
                        {
                            DeviceType = "DEVICETYPE",
                            DeviceNumber = 20652
                        },
                        Header = new RequestHeader
                        {
                            MessageID = rdcCallParams.ReceiptTransactionNumber
                        },
                        Body = new RequestBody
                        {
                            File = "KCU.SYC.MobileDeposit",
                            RGSession = 1,
                            UserDefinedParameters = new UserDefinedParameters
                            {
                                RGUserChr = new List<RGUserChr>()
                                {
                                    new RGUserChr
                                    {
                                        ID = 1,
                                        Value = rdcCallParams.CodeLine
                                    },
                                    new RGUserChr
                                    {
                                        ID = 2,
                                        Value = rdcCallParams.HostHoldCode
                                    },
                                    new RGUserChr
                                    {
                                        ID = 3,
                                        Value = rdcCallParams.ReceiptTransactionNumber
                                    }
                                },
                                RGUserNum = new List<RGUserNum>()
                                {
                                    new RGUserNum
                                    {
                                        ID = 1,
                                        Value = (int)(rdcCallParams.Amount * 100) // Amount in cents
                                    }
                                }
                            },
                            User = "821"
                        }
                    }
                }
            }
        };
    }
}