using System.Text.RegularExpressions;

namespace VHS.Services.Farming.Helpers;

public static class LayerSortingHelper
{
    /// <summary>
    /// Extracts numeric sorting keys from a variable-length code like "SK1-2-3-4".
    /// Handles codes with any number of groups dynamically.
    /// </summary>
    public static List<int> ExtractSortKey(string? code, string prefix = "SK")
    {
        if (string.IsNullOrEmpty(code))
            return new List<int> { int.MaxValue };

        // Match pattern dynamically to support SK1, SK1-1, SK1-1-1-1, etc.
        string pattern = $@"{Regex.Escape(prefix)}(\d+(?:-\d+)*)";
        var match = Regex.Match(code, pattern);

        if (match.Success)
        {
            return match.Groups[1].Value
                .Split('-')
                .Select(num => int.TryParse(num, out var n) ? n : int.MaxValue) // Parse or use max value
                .ToList();
        }

        return new List<int> { int.MaxValue };
    }

    /// <summary>
    /// Sorts a collection of LayerDTOs based on their extracted numeric sort key.
    /// </summary>
    /// Will be removed as Layer as been refactored to store Code in RackCell
    //public static List<LayerDTO> SortLayers(IEnumerable<LayerDTO> layers, bool ascending = true, string prefix = "SK")
    //{
    //    return ascending
    //        ? layers.OrderBy(l => ExtractSortKey(l.Code, prefix), new ListComparer()).ToList()
    //        : layers.OrderByDescending(l => ExtractSortKey(l.Code, prefix), new ListComparer()).ToList();
    //}

    /// <summary>
    /// Sorts a collection of TrayDTOs based on their extracted numeric sort key from LayerCode.
    /// </summary>
    //public static List<TrayDTO> SortLayersFromTrays(IEnumerable<TrayDTO> layers, bool ascending = true, string prefix = "SK")
    //{
    //    return ascending
    //        ? layers.OrderBy(l => ExtractSortKey(l.RackCellCode, prefix), new ListComparer()).ToList()
    //        : layers.OrderByDescending(l => ExtractSortKey(l.RackCellCode, prefix), new ListComparer()).ToList();
    //}

    //public static List<RackCellDTO> SortLayersFromRackCells(IEnumerable<RackCellDTO> layers, bool ascending = true, string prefix = "SK")
    //{
    //    return ascending
    //        ? layers.OrderBy(l => ExtractSortKey(l.Code, prefix), new ListComparer()).ToList()
    //        : layers.OrderByDescending(l => ExtractSortKey(l.Code, prefix), new ListComparer()).ToList();
    //}
}

/// <summary>
/// Custom comparer to compare lists of integers for sorting.
/// </summary>
//public class ListComparer : IComparer<List<int>>
//{
//    public int Compare(List<int>? x, List<int>? y)
//    {
//        if (x == null || y == null) return 0;

//        int minLength = Math.Min(x.Count, y.Count);

//        for (int i = 0; i < minLength; i++)
//        {
//            int comparison = x[i].CompareTo(y[i]);
//            if (comparison != 0)
//                return comparison;
//        }

//        return x.Count.CompareTo(y.Count); // Compare lengths if all previous elements are equal
//    }
//}
