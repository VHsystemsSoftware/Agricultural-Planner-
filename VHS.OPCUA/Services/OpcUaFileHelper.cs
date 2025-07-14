using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VHS.OPCUA.Models;
using Workstation.ServiceModel.Ua;

namespace VHS.OPCUA.Services;

/// <summary>
/// Helper methods for reading/writing OPC-UA server settings and log files.
/// </summary>
public static class OpcUaFileHelper
{
    /// <summary>
    /// Reads server settings JSON file if exists; otherwise returns a new OpcUaServer instance.
    /// </summary>
    public static OpcUaServer? ReadJsonServerSettings(ILogger logger,string file)
    {
        try
        {
            if (string.IsNullOrEmpty(file)) throw new ArgumentNullException(nameof(file));

            var serializer = new JsonSerializer();
            var path = string.Format(file);

            logger.LogInformation($"ReadJsonServerSettings - {path}");

            if (File.Exists(path))
            {
                using var fs = File.Open(path, FileMode.Open);
                using var sr = new StreamReader(fs);
                using var reader = new JsonTextReader(sr);
                if (!sr.EndOfStream)
                {
                    return serializer.Deserialize<OpcUaServer>(reader)!;
                }
            }

            // No existing file, return a new instance
            return null;
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            return null;
        }
    }

    ///// <summary>
    ///// Adds a log entry to the list and writes to console.
    ///// </summary>
    //public static void LogInfo(ILogger logger, string message)
    //{
    //    logger.LogInformation(message);
    //    //loggingList.Add(message);
    //    //Console.WriteLine(message);
    //}

    /// <summary>
    /// Logs detailed node information into the list.
    /// </summary>
    //public static void LogNodeInformation(
    //    List<string> loggingList,
    //    ReferenceDescription? rdGeneral,
    //    int level)
    //{
    //    var indent = new string(' ', level * 2);
    //    if (rdGeneral != null)
    //    {
    //        loggingList.Add(
    //            $"LogNodeInformation - {level}{indent}+ {rdGeneral.DisplayName}: {rdGeneral.BrowseName}, " +
    //            $"{rdGeneral.NodeClass}, nodeId: {rdGeneral.NodeId}, datatype: {rdGeneral.TypeDefinition}");
    //    }
    //}
}
