namespace VHS.Services.Common.DataGrid;

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
