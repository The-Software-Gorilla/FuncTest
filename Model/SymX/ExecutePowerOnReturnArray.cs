using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Azure.Core;

namespace FuncTest.Model.SymX;

public class ExecutePowerOnReturnArray
{
    [XmlElement(ElementName = "Request", Namespace = "")]
    [JsonPropertyName("request")]
    public Request Request { get; set; }    
}