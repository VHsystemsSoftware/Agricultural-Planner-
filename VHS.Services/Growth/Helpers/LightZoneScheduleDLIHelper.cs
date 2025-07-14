namespace VHS.Services.Growth.Helpers;

public static class LightZoneConstant
{
    /// <summary>
    /// Maximum Photosynthetic Photon Flux Density (MaxPPFD).
    /// It represents the highest intensity of photosynthetically active radiation (PAR)
    /// that a light source can deliver, measured in micromoles of photons per square meter per second (µmol/m²/s).
    /// 
    /// Controlled Environment (Greenhouses/Indoor Farming)
    /// Many indoor facilities using LED grow lights target values in the range of 200–600 µmol/m²/s, with 500 µmol/m²/s often used as a mid-range estimate for many crops.
    /// </summary>
    public const double MAXPPFD = 500;
}

public static class LightZoneScheduleDLIHelper
{
    /// <summary>
    /// Calculates the Daily Light Integral (DLI) based on intensity, start time, and end time.
    /// </summary>
    public static double CalculateDLI(decimal intensity, TimeOnly startTime, TimeOnly endTime)
    {
        // Calculate the duration; if end time is before start time, assume it crosses midnight.
        TimeSpan duration = endTime.ToTimeSpan() - startTime.ToTimeSpan();
        if (duration < TimeSpan.Zero)
        {
            duration = duration.Add(TimeSpan.FromDays(1));
        }

        // Convert total seconds of light to decimal.
        decimal secondsOfLight = Convert.ToDecimal(duration.TotalSeconds);

        // Convert the maximum PPFD constant to decimal (assuming LightZoneConstant.MaxPPFD is double).
        decimal maxPPFD = Convert.ToDecimal(LightZoneConstant.MAXPPFD);

        // Adjusted PPFD is the maximum PPFD scaled by the intensity (percentage).
        decimal adjustedPPFD = (intensity / 100m) * maxPPFD;

        // DLI (mol/m²/d) = (adjusted PPFD in µmol/m²/s * seconds of light) / 1,000,000.
        decimal dli = (adjustedPPFD * secondsOfLight) / 1_000_000m;

        return (double)dli;
    }
}
