using System.Text.Json;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using FuncTest.Model.SymX;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FuncTest.Triggers;

public class RdcSymxCallBuilder
{
    private readonly ILogger<RdcSymxCallBuilder> _logger;
    private readonly string _storageConn;
    private const string SymxPowerOn = "KCU.SYC.MobileDeposit";
    private const string SymxInstanceUrl = "RDC";
    private const string SymxUser = "821";
    private const string SymxDeviceType = "DEVICETYPE";
    private const int SymxDeviceNumber = 20652;
    private const string RdcCallbackQueue = "deposit-callback";
    private const string SymxOutboundQueue = "symx-outbound";


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
        
        var rdcCallParams = JsonSerializer.Deserialize<Model.RDCSystem.RDCForwarder>(message.MessageText);

        var rdcCall = CreateSymXCall(rdcCallParams);
        
        var symXCall = new SymXCall
        {
            SymXCallId = Guid.NewGuid().ToString(),
            CorrelationId = rdcCallParams.TransactionId,
            CallbackQueue = RdcCallbackQueue,
            SymXInstanceUrl = SymxInstanceUrl,
            SymXPowerOn = SymxPowerOn,
            SymXEnvelope = rdcCall
        };
        
        var json = JsonSerializer.Serialize(symXCall, new JsonSerializerOptions { WriteIndented = true });
        
        var queueClient = new QueueClient(_storageConn, SymxOutboundQueue, new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        });
        await queueClient.CreateIfNotExistsAsync();
        await queueClient.SendMessageAsync(json);
        
        _logger.LogInformation("RDC SymX Call XML for Transaction ID {TransactionId}:\n{Xml}", rdcCallParams.TransactionId, json);
        
    }
    
    private static SymXSoapEnvelope CreateSymXCall(Model.RDCSystem.RDCForwarder rdcCallParams)
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
                            ProcessorUser = SymxUser,
                            AdministrativeCredentials = new AdministrativeCredentials()
                            {
                                Password = "PASSWORD"
                            }
                        },
                        DeviceInformation = new DeviceInformation
                        {
                            DeviceType = SymxDeviceType,
                            DeviceNumber = SymxDeviceNumber
                        },
                        Header = new RequestHeader
                        {
                            MessageID = rdcCallParams.ReceiptTransactionNumber
                        },
                        Body = new RequestBody
                        {
                            File = SymxPowerOn,
                            RGSession = 1,
                            UserDefinedParameters = new UserDefinedParameters
                            {
                                RGUserChr = new List<RGUserChr>
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
                                RGUserNum = new List<RGUserNum>
                                {
                                    new RGUserNum
                                    {
                                        ID = 1,
                                        Value = (int)(rdcCallParams.Amount * 100) // Amount in cents
                                    }
                                }
                            },
                            User = SymxUser
                        }
                    }
                }
            }
        };
    }
}