using Newtonsoft.Json;
using System;
using System.Reactive;
using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace VHS.OPCUA.Models;

public class OpcUaServer
{
    public string HostnameOrIP { get; set; }
    public int PortNumber { get; set; }
    public int ServerId { get; set; }
    public string DiscoveryUrl => $"opc.tcp://{HostnameOrIP}:{PortNumber}";
    public DateTime? LastBrowseDate { get; set; }
    public List<OpcUaComponentNode> Components { get; set; }
    public string NamespaceUrn { get; set; }

    [JsonIgnore] public int NamespaceIndex
    {
        get
        {
            if (this.ClientSession != null)
            {
                IList<string> namespaceUris = this.ClientSession.NamespaceUris.ToArray();

                // Search for index
                return namespaceUris.IndexOf(this.NamespaceUrn);
            }
            else
            {
                return -1; // Return -1 if session is null
            }
        }
    }
    [JsonIgnore] public ClientSessionChannel? ClientSession { get; set; }
    [JsonIgnore] private string ClientName { get; set; }
    [JsonIgnore] private string Username { get; set; }
    [JsonIgnore] private string Password { get; set; }
    [JsonIgnore] public uint SubscriptionId { get; set; }
    [JsonIgnore] public IDisposable? Token { get; set; }

    public OpcUaServer()
    {
        HostnameOrIP = string.Empty;
        PortNumber = 0;
        Components = new List<OpcUaComponentNode>();
    }

    public OpcUaServer(string hostnameOrIP, int portNumber, int serverId) : this()
    {
        HostnameOrIP = hostnameOrIP;
        PortNumber = portNumber;
        ServerId = serverId;
    }

    public bool IsBrowseNeeded()
    {
        if (!LastBrowseDate.HasValue) return true;
        return (DateTime.UtcNow - LastBrowseDate.Value).TotalHours > 24;
    }

    public void CreateSession(string clientName, string userName, string password)
    {
        try
        {
            ClientName = clientName;
            Username = userName;
            Password = password;

            var appDesc = new ApplicationDescription
            {
                ApplicationName = clientName,
                ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:{clientName}",
                ApplicationType = ApplicationType.Client
            };

            var pkiPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), clientName, "pki");
            var certificateStore = new DirectoryStore(pkiPath);
            IUserIdentity userIdentity = new UserNameIdentity(userName, password);

            ClientSession = new ClientSessionChannel(appDesc, certificateStore, userIdentity, DiscoveryUrl);
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
        }
    }

    public void RecreateSession() => CreateSession(ClientName, Username, Password);

    public async Task CreateSubscriptionAsync()
    {
        try
        {
            if (ClientSession != null)
            {
                var request = new CreateSubscriptionRequest
                {
                    RequestedPublishingInterval = 10,
                    RequestedMaxKeepAliveCount = 20,
                    RequestedLifetimeCount = 30,
                    PublishingEnabled = true
                };

                //using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
                var response = await ClientSession.CreateSubscriptionAsync(request);
                SubscriptionId = response.SubscriptionId;
            }
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
		}
    }

    public async System.Threading.Tasks.Task<bool> DeleteSubscriptionAsync()
    {
        try
        {
            if (ClientSession != null && ClientSession.Completion.Status == System.Threading.Tasks.TaskStatus.Running)
            {
                var request = new DeleteSubscriptionsRequest { SubscriptionIds = new[] { SubscriptionId } };
                await ClientSession.DeleteSubscriptionsAsync(request);
                Token?.Dispose();
                return true;
            }
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
        }
        return false;
    }

    public async Task<ReadResponse> ReadNodeDataAsync(List<string> listOfNodes)
    {
        try
		{
			ReadValueId?[] items = new ReadValueId?[listOfNodes.Count()];
			int count = 0;

			foreach (string nodeValue in listOfNodes)
			{
				ReadValueId value = new()
				{
					NodeId = NodeId.Parse(nodeValue),
					AttributeId = AttributeIds.Value
				};

				items[count] = value;

				count++;
			}

			ReadRequest readRequest = new()
			{
				NodesToRead = items
			};

			await CheckConnection();

			// send the ReadRequest to the server.
			var readResult = await this.ClientSession.ReadAsync(readRequest);

			return readResult;
		}
		catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);

			return null;
		}
    }

	private async Task CheckConnection()
	{
		if (this.ClientSession == null || this.ClientSession.State != CommunicationState.Opened)
		{
			while (this.ClientSession.State != CommunicationState.Opened)
			{
				//_logger.LogInformation("Waiting for session to open: {Url}", server.DiscoveryUrl);
				try
				{
					await this.ClientSession.OpenAsync();
				}
				catch (Exception)
				{
					this.RecreateSession();
					await Task.Delay(3000);
				}
			}
		}
	}

	public async Task<WriteResponse> WriteNodeDataAsync(string nodeId, Variant value)
    {
        try
        {
            var writeRequest = new WriteRequest
            {
                // set the NodesToRead to an array of ReadValueIds.
                NodesToWrite = new[] {
                    // construct a ReadValueId from a NodeId and AttributeId.
                    new WriteValue {
                        NodeId = NodeId.Parse(nodeId),
                        AttributeId = AttributeIds.Value,
                        Value = new DataValue(value)
                    }
                }
            };

			await CheckConnection();

			// send the ReadRequest to the server.
			WriteResponse writeResult = await this.ClientSession.WriteAsync(writeRequest);

            return writeResult;
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);

            return null;
        }
    }

    public async Task<WriteResponse> WriteNodeDataAsync(List<OpcUaNodeWriteValue> listOfNodes)
    {
        try
        {
            WriteRequest writeRequest = new()
            {
                NodesToWrite = new WriteValue?[listOfNodes.Count()]
            };

            int index = 0;
            foreach (OpcUaNodeWriteValue nodeValue in listOfNodes)
            {
                object valueToWrite = nodeValue.Value.Value;

                if (valueToWrite is int || valueToWrite is double || valueToWrite is bool || valueToWrite is string || valueToWrite is short || valueToWrite is ushort || valueToWrite is uint)
                {
                    writeRequest.NodesToWrite[index++] = new WriteValue
                    {
                        NodeId = NodeId.Parse(nodeValue.NodeId),
                        AttributeId = AttributeIds.Value,
                        Value = new DataValue(new Variant(valueToWrite))
                    };
                }
                else
                {
                    throw new ArgumentException($"Unsupported type: {valueToWrite.GetType()}");
                }
            }

			if (this.ClientSession == null || this.ClientSession.State != CommunicationState.Opened)
			{
				while (this.ClientSession.State != CommunicationState.Opened)
				{
					//_logger.LogInformation("Waiting for session to open: {Url}", server.DiscoveryUrl);
					try
					{
						await this.ClientSession.OpenAsync();
					}
					catch (Exception)
					{
						this.RecreateSession();
						await Task.Delay(3000);
					}
				}
			}

			// send the WriteResponse to the server.
			WriteResponse writeResult = await this.ClientSession.WriteAsync(writeRequest);

            // check if all result items have statuscode 'Good'
            bool allWritesSuccessful = writeResult.Results.All(status => StatusCode.IsGood(status));

            if (!allWritesSuccessful)
            {
                for (int y = 0; y < writeResult.Results.Length; y++)
                {
                    var status = writeResult.Results[y];
                    if (!StatusCode.IsGood(status))
                    {
                        throw new Exception($"OPC server: {this.DiscoveryUrl}, WriteNodeData item {listOfNodes[y].NodeId} {y}: {status}");
                    }
                }
            }

            return writeResult;
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            return null;
        }
    }

    public string GetVariableNodeName(string handleId)
    {
        try
        {
            foreach (OpcUaComponentNode comp in this.Components)
            {
                {
                    OpcUaVariableNodeItem? temp = comp.Variables.SingleOrDefault(item => item.HandleId.ToString() == handleId);

                    if (temp != null)
                    {
                        return temp.Node;
                    }
                }
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            return string.Empty;
		}
    }

    public IList<OpcUaVariableNodeItem> GetVariablesToSubscribe()
    {
        try
        {
            List<OpcUaVariableNodeItem> returnList = [];

            foreach (OpcUaComponentNode comp in this.Components)
            {
                {
                    returnList.AddRange([.. comp.Variables.Where(vars => vars.MustSubscribe == true)]);
                }
            }

            return returnList;
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            return null;
        }
    }

    public (OpcUaComponentNode, OpcUaVariableNodeItem) GetVariableByHandleId(int handleId)
    {
        try
        {
            OpcUaComponentNode returnComp = null;
            OpcUaVariableNodeItem returnVar = null;

            foreach (OpcUaComponentNode comp in this.Components)
            {
                {
                    OpcUaVariableNodeItem item = comp.Variables.Where(vars => vars.HandleId == handleId).SingleOrDefault<OpcUaVariableNodeItem>();

                    if (item != null)
                    {
                        returnComp = comp;
                        returnVar = item;
                        break;
                    }
                }
            }

            return (returnComp, returnVar);
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
			return (null, null);
		}
    }

    public string GetComponentVarName(uint handleId)
    {
        try
        {
            foreach (OpcUaComponentNode comp in this.Components)
            {
                OpcUaVariableNodeItem? item = comp.Variables.SingleOrDefault(vars => vars.HandleId == handleId);
                if (item != null)
                {
                    return $"{comp.Name}.{item.Name}";
                }
            }

            return "unknown";
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
			return null;
		}
    }
}