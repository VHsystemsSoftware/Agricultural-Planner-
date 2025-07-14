using Opc.Ua;

namespace VHS.OPCUA.Models;

public class OpcUaNodeWriteValue
{
    public string NodeId { get; set; } = string.Empty;
    public Variant Value { get; set; }

    public OpcUaNodeWriteValue() { }

    public OpcUaNodeWriteValue(string nodeId, Variant value)
    {
        NodeId = nodeId;
        Value = value;
    }
}
