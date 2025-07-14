using System.Collections.Generic;

namespace VHS.OPCUA.Models;

public class OpcUaComponentNode
{
    public string Name { get; set; }
    public string Node { get; set; }
    public int Id { get; set; }
    public List<OpcUaVariableNodeItem> Variables { get; set; }

    public OpcUaComponentNode()
    {
        Name = string.Empty;
        Node = string.Empty;
        Variables = new List<OpcUaVariableNodeItem>();
        Id = 0;

    }
}
