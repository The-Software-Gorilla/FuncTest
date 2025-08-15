using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace FuncTest.Model.SymX;

public class AdministrativeCredentials
{
    [XmlElement(ElementName = "Password")]
    [JsonPropertyName("password")]
    public string Password { get; set; }
    
}