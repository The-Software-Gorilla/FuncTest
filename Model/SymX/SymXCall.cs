using System.Text.Json.Serialization;

namespace FuncTest.Model.SymX;

public class SymXCall
{
    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; }
    
    [JsonPropertyName("callbackQueue")]
    public string CallbackQueue { get; set; }
    
    [JsonPropertyName("symXInstance")]
    public string SymXInstance { get; set; }
    
    [JsonPropertyName("symXEnvelope")]
    public SymXSoapEnvelope SymXEnvelope { get; set; }
}