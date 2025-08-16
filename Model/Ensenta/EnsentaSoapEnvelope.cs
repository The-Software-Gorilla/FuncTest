using System.Xml.Serialization;

namespace Tsg.Rdc.Model.Ensenta;

[XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class EnsentaSoapEnvelope
{
    [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public EnsentaSoapBody Body { get; set; }
}