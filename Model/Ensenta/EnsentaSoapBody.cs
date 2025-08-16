using System.Xml.Serialization;

namespace Tsg.Rdc.Model.Ensenta;

public class EnsentaSoapBody
{
    [XmlElement(ElementName = "DoDepositTransaction", Namespace = "http://ensenta.com/ECWebDepositHostRequest")]
    public DoDepositTransaction DoDepositTransaction { get; set; }
}