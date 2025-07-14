using System;
using System.Collections.Generic;
using Opc.Ua;

namespace VHS.OPCUA.Models;

public enum SignalType
{
    None = 0,
    ControlSignal = 1,
    Status = 2
}
public class OpcUaVariableNodeItem
{
	public string Name { get; set; }

	public string Node { get; set; }

	public SignalType Type { get; set; }

	public bool MustSubscribe { get; set; } = false;

	public int Id { get; set; }

	public int HandleId { get; set; }

	public OpcUaVariableNodeItem(string name, string node, SignalType type, bool mustSubscribe, int serverId, int componentId, int variableId)
	{
		Name = name;
		Node = node;
		Type = type;
		MustSubscribe = mustSubscribe;
		Id = variableId;

		SetHandleId(serverId, componentId);
	}

	public void SetHandleId(int serverId, int componentId)
	{
		try
		{
			string handleId = $"{serverId}{componentId.ToString().PadLeft(2, '0')}{this.Id.ToString().PadLeft(2, '0')}";

			_ = int.TryParse(handleId, out int id);

			this.HandleId = id;			
		}
		catch (Exception ex)
		{
			SentrySdk.CaptureException(ex);

			throw;
		}
	}
}
public class NodeWriteValue
{
	public string NodeId { get; set; } = string.Empty;

	public Variant Value { get; set; }

}