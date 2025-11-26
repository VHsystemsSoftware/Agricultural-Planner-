namespace VHS.Services.Growth.Helpers;

public static class WaterZoneScheduleDWRHelper
{
    /// <summary>
    /// Calculates the Daily Water Requirement (DWR) in liters per day.
    /// Assumes that the provided volume (in liters) is applied over the watering period.
    /// If the watering period crosses midnight, the duration is adjusted accordingly.
    /// Then, the water flow rate (liters per hour) is extrapolated to a full 24-hour day.
    /// </summary>
    /// <param name="volume">The volume of water applied during the watering period (in liters).</param>
    /// <param name="startTime">The start time of the watering period.</param>
    /// <param name="endTime">The end time of the watering period.</param>
    /// <returns>The calculated daily water requirement in liters per day, or 0 if the duration is invalid.</returns>
    public static double CalculateDWR(decimal volume, TimeOnly startTime, TimeOnly endTime)
    {
        // Calculate the duration; if endTime is before startTime, assume it crosses midnight.
        TimeSpan duration = endTime.ToTimeSpan() - startTime.ToTimeSpan();
        if (duration < TimeSpan.Zero)
        {
            duration = duration.Add(TimeSpan.FromDays(1));
        }

        // Convert the total hours from double to decimal for consistent arithmetic.
        decimal hours = Convert.ToDecimal(duration.TotalHours);
        if (hours <= 0)
        {
            return 0;
        }

        // Calculate water flow rate (liters per hour) using decimal arithmetic.
        decimal flowRate = volume / hours;
        // Extrapolate to a full 24-hour day.
        decimal dwr = flowRate * 24m;
        return (double)dwr;
    }
}
