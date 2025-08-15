using System.Xml.Serialization;

namespace FuncTest.Messages;

public class SoapBody
{
    [XmlElement(ElementName = "DoDepositTransaction", Namespace = "http://ensenta.com/ECWebDepositHostRequest")]
    public DoDepositTransaction DoDepositTransaction { get; set; }
}