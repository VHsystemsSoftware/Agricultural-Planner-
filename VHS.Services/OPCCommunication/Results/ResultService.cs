using Microsoft.EntityFrameworkCore;
using VHS.Services.Results.DTO;
using VHS.Services.Audit;

namespace VHS.Services.Results;

public interface IResultService
{
	Task<List<ResultItemDTO>> GetResultsAsync(ResultFilterDTO filter);
}

public static class ResultItemDTOSelect
{
	public static IQueryable<ResultItemDTO> MapToDTO(this IQueryable<TrayState> query)
	{
		var method = System.Reflection.MethodBase.GetCurrentMethod();
		return query.TagWith(method?.Name ?? "MapToDTO")
			.Select(ts => new ResultItemDTO
			{
				TrayTag = ts.Tray.Tag,
				SeedDate = ts.SeedDate.Value,
				PlannedHarvestDate = ts.GrowFinishedDate.Value,
				RealHarvestDate = DateOnly.FromDateTime(ts.ArrivedHarvest.Value),
				WeightKg = ts.HarvestedWeightKG ?? 0,
				BatchName = ts.Batch.Name,
				ProductName = ts.Recipe.Product.Name,
				AddedDateTime = ts.AddedDateTime,
				WeightDateTime = ts.WeightRegistered.GetValueOrDefault(ts.ArrivedHarvest.GetValueOrDefault())
			});
	}
}

public class ResultService : IResultService
{
	private readonly IUnitOfWorkCore _unitOfWork;

	public ResultService(IUnitOfWorkCore unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<List<ResultItemDTO>> GetResultsAsync(ResultFilterDTO filter)
	{
		try
		{
			var query = _unitOfWork.TrayState
				.Query(x => x.RecipeId.HasValue && x.ArrivedHarvest != null && x.HarvestedWeightKG != null)
				.Include(ts => ts.Tray)
				.Include(ts => ts.Batch.GrowPlan)
				.Include(ts => ts.Recipe)
				.IgnoreQueryFilters()
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(filter.TrayTag))
				query = query.Where(ts => ts.Tray.Tag.Contains(filter.TrayTag));

			if (filter.SeedFrom.HasValue)
				query = query.Where(ts => ts.SeedDate >= filter.SeedFrom.Value);

			if (filter.SeedTo.HasValue)
				query = query.Where(ts => ts.SeedDate <= filter.SeedTo.Value);

			if (filter.PlannedHarvestFrom.HasValue)
				query = query.Where(ts => ts.GrowFinishedDate >= filter.PlannedHarvestFrom.Value);

			if (filter.PlannedHarvestTo.HasValue)
				query = query.Where(ts => ts.GrowFinishedDate <= filter.PlannedHarvestTo.Value);

			if (filter.RealHarvestFrom.HasValue)
				query = query.Where(ts => DateOnly.FromDateTime(ts.ArrivedHarvest.Value) >= filter.RealHarvestFrom.Value);

			if (filter.RealHarvestTo.HasValue)
				query = query.Where(ts => DateOnly.FromDateTime(ts.ArrivedHarvest.Value) <= filter.RealHarvestTo.Value);

			return await query
				.MapToDTO()
				.AsNoTracking()
				.ToListAsync();
		}
		catch (Exception ex)
		{
			throw;
		}
	}
}
