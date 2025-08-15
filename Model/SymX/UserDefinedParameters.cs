using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace FuncTest.Model.SymX;

public class UserDefinedParameters
{
    [XmlElement(ElementName = "RGUserChr")]
    [JsonPropertyName("rgUserChr")]
    public List<RGUserChr> RGUserChr { get; set; }

    [XmlElement(ElementName = "RGUserNum")]
    [JsonPropertyName("rgUserNum")]
    public List<RGUserNum> RGUserNum { get; set; }
}
