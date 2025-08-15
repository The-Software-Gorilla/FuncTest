using System.Text.Json.Serialization;

namespace FuncTest.Model.RDCSystem;

public class RDCForwarder
{
    [JsonPropertyName("transactionId")]
    public string TransactionId { get; set; }
    
    [JsonPropertyName("receiptTransactionNumber")]
    public string ReceiptTransactionNumber { get; set; }
    
    [JsonPropertyName("codeLine")]
    public string CodeLine { get; set; }
    
    [JsonPropertyName("hostHoldCode")]
    public string HostHoldCode { get; set; }
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("depositItemCount")]
    public int DepositItemCount { get; set; }
    
    
}