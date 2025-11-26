using System.Text.Json.Serialization;

namespace VHS.OPC.Models.Message.Request;

public abstract class JsonRpcBaseRequest<T>
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("jsonrpc")]
    public string Jsonrpc { get; set; } = "2.0";

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public T Data { get; set; }
}