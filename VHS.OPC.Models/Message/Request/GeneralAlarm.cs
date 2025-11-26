using System.Text.Json.Serialization;
using VHS.OPC.Models.Message;

namespace VHS.OPC.Models.Message.Request
{
    public class GeneralAlarmStatusRequest : BaseRequest<GeneralAlarmStatusRequestData>
    {
        public GeneralAlarmStatusRequest() : this(default) { }

        public GeneralAlarmStatusRequest(MessageType type) : base(type)
        {
        }
    }

    public class GeneralAlarmStatusRequestData
    {
        [JsonPropertyName("Value")]
        public VarAlarmIsActive Value { get; set; }
    }
}