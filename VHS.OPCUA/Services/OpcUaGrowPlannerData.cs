// File: Services/OpcUaGrowPlannerData.cs
using System;

namespace VHS.OPCUA.Services;

/// <summary>
/// Simulation of retrieving and updating data from the Grow Planner application.
/// TODO: Replace simulations with actual data retrieval/persistence (e.g., JSON file, database).
/// </summary>
public static class OpcUaGrowPlannerData
{
    /// <summary>
    /// Simulates fetching rack and layer info for a given tray ID.
    /// </summary>
    public static (int rackId, int layer) GetTrayData(int trayId)
    {
        int rackId = trayId % 10;
        int layer = 5;
        return (rackId, layer);
    }

    /// <summary>
    /// Simulates whether harvesting is allowed for a given tray ID.
    /// </summary>
    public static bool GetTrayDataHarvester(int trayId)
    {
        // TODO: Implement actual logic based on batch plan state
        return true;
    }

    /// <summary>
    /// Simulates computing the destination floor for a Paternoster tray.
    /// </summary>
    public static int GetTrayDataPaternoster(int trayId)
    {
        // e.g., 2: SK2, 3: SK3
        return 2;
    }

    /// <summary>
    /// Simulates retrieving seeder output details.
    /// </summary>
    public static (int rackId, int layer, uint trayId, int destinationOutput) GetTrayDataSeeder(int trayId)
    {
        int rackId = 3;
        int layer = 5;
        int destinationOutput = -1;
        uint outputTrayId = (uint)trayId;
        return (rackId, layer, outputTrayId, destinationOutput);
    }

    /// <summary>
    /// Simulates marking a tray as accepted in the batch plan.
    /// </summary>
    public static void SetTrayDataAccept(int trayId)
    {
        int rackId = trayId % 10;
        int layer = 5;
        // TODO: Update batch plan persistence here
    }
}
