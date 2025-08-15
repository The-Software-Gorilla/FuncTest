using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace FuncTest.Messages;

public class DepositItem
{
    [XmlElement]
    [JsonPropertyName("Amount")]
    public string Amount { get; set; }

    [XmlElement]
    [JsonPropertyName("CodeLine")]
    public string CodeLine { get; set; }

    [XmlElement]
    [JsonPropertyName("HostHoldCode")]
    public string HostHoldCode { get; set; }

    [XmlElement]
    [JsonPropertyName("FrontFileContents")]
    public string FrontFileContents { get; set; }

    [XmlElement]
    [JsonPropertyName("BackFileContents")]
    public string BackFileContents { get; set; }
}