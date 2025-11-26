using System.Text.Json.Serialization;

namespace VHS.OPC.Models.Message.Request
{
    public class GeneralAlarmRequest : JsonRpcBaseRequest<GeneralAlarmRequestData>
    {
    }

    public class GeneralAlarmRequestData
    {
        [JsonPropertyName("Value")]
        public VarAlarmIsActive Value { get; set; }
    }
}
