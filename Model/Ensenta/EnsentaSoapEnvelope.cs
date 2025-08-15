using System.Xml.Serialization;

namespace FuncTest.Model.Ensenta;

[XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class EnsentaSoapEnvelope
{
    [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public EnsentaSoapBody Body { get; set; }
}