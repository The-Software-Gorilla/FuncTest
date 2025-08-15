using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace FuncTest.Model.Ensenta;

public class DoDepositTransaction
{
    [XmlElement]
    [JsonPropertyName("accountHolderNumber")]
    public string accountHolderNumber { get; set; }

    [XmlElement]
    [JsonPropertyName("acctSuffix")]
    public string acctSuffix { get; set; }

    [XmlElement]
    [JsonPropertyName("receiptTransactionNumber")]
    public string receiptTransactionNumber { get; set; }

    [XmlElement]
    [JsonPropertyName("stationDateTime")]
    public string stationDateTime { get; set; }

    [XmlElement]
    [JsonPropertyName("isReversalFlag")]
    public string isReversalFlag { get; set; }

    [XmlElement]
    [JsonPropertyName("transactionType")]
    public string transactionType { get; set; }

    [XmlElement]
    [JsonPropertyName("feeAmount")]
    public string feeAmount { get; set; }

    [XmlArray("depositItems")]
    [XmlArrayItem("DepositItem")]
    [JsonPropertyName("depositItems")]
    public List<DepositItem> depositItems { get; set; }
}