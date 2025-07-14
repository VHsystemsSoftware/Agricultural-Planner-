namespace VHS.OPCUA.Configuration
{
    public class OpcUaConfiguration
    {
        public bool IsSimulation { get; set; } = false;
		public string EndpointUrl { get; set; } = "opc.tcp://localhost:4840";
        public int SessionTimeout { get; set; } = 60000;
        public string LogFolder { get; set; } = "Logs/OpcUa";
    }
}