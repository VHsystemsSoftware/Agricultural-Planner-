using Microsoft.EntityFrameworkCore;
using VHS.Common;
using VHS.Data.Core.Infrastructure;
using VHS.Services.Farming.DTO;
using VHS.Services.Home.DTO;
using VHS.Services.SystemMessages.DTO;
using System.Linq;

namespace VHS.Services.Home;

public interface IHomeService
{
	Task<PlanningStatsDTO> GetPlanningStatsAsync();
	Task<OperationalStatsDTO> GetOperationalStatsAsync();
	Task<ResultsStatsDTO> GetResultsStatsAsync(bool usePounds = false);
	Task<List<SystemMessageDTO>> GetSystemMessagesAsync();
}

public class HomeService : IHomeService
{
	private readonly IUnitOfWorkCore _unitOfWorkCore;
	private readonly IUnitOfWorkAudit _unitOfWorkAudit;
	private readonly IUserSettingService _userSettingService;
	private readonly ITrayStateService _trayStateService;

	public HomeService(IUnitOfWorkCore unitOfWorkCore, IUserSettingService userSettingService, ITrayStateService trayStateService, IUnitOfWorkAudit unitOfWorkAudit)
	{
		_unitOfWorkCore = unitOfWorkCore;
		_userSettingService = userSettingService;
		_trayStateService = trayStateService;
		_unitOfWorkAudit = unitOfWorkAudit;
	}

	public async Task<PlanningStatsDTO> GetPlanningStatsAsync()
	{
		var today = DateOnly.FromDateTime(DateTime.UtcNow);
		var tomorrow = today.AddDays(1);
		var todayStart = DateTime.UtcNow.Date;
		var tomorrowStart = todayStart.AddDays(1);

		var allLayers = await _unitOfWorkCore.Layer.Query(x => x.Enabled && x.Rack.Enabled && x.Rack.Floor.Enabled).Include(l => l.Rack).ToListAsync();
		var allTrayStates = await _trayStateService.GetCurrentStates();

		var totalCapacity = allLayers.Sum(l => l.Rack.TrayCountPerLayer);

		var traysFinishedToday = await _unitOfWorkCore.TrayState
			.Query(ts => ts.GrowFinishedDate.HasValue &&
						 ts.GrowFinishedDate.Value == today)
			.CountAsync();

		var traysPlannedPushOut = await _unitOfWorkCore.TrayState
			.Query(ts => ts.WillBePushedOutGrow.HasValue &&
						 ts.WillBePushedOutGrow.Value >= todayStart &&
						 ts.WillBePushedOutGrow.Value < tomorrowStart)
			.CountAsync();

		return new PlanningStatsDTO
		{
			GerminationOccupancy = CalculateOccupancy(allLayers, allTrayStates, GlobalConstants.RACKTYPE_GERMINATION),
			PropagationOccupancy = CalculateOccupancy(allLayers, allTrayStates, GlobalConstants.RACKTYPE_PROPAGATION),
			GrowingOccupancy = CalculateOccupancy(allLayers, allTrayStates, GlobalConstants.RACKTYPE_GROWING),
			PercentageFinishedToday = totalCapacity > 0 ? (double)traysFinishedToday / totalCapacity * 100 : 0,
			PercentagePlannedPushOut = totalCapacity > 0 ? (double)traysPlannedPushOut / totalCapacity * 100 : 0
		};
	}

	private double CalculateOccupancy(List<Layer> layers, IEnumerable<TrayStateDTO> trayStates, Guid rackTypeId)
	{
		var relevantLayers = layers.Where(l => l.Rack.TypeId == rackTypeId);
		double totalSpots = relevantLayers.Sum(l => l.Rack.TrayCountPerLayer);
		if (totalSpots == 0) return 0;

		var relevantLayerIds = relevantLayers.Select(l => l.Id).ToHashSet();
		double filledSpots = trayStates.Count(ts =>
		ts.RecipeId.HasValue && (
			(ts.GrowLayerId.HasValue && ts.GrowOrderOnLayer > 0 && relevantLayerIds.Contains(ts.GrowLayerId.Value))
			|| (ts.PreGrowLayerId.HasValue && ts.PreGrowOrderOnLayer > 0 && relevantLayerIds.Contains(ts.PreGrowLayerId.Value))));

		return (filledSpots / totalSpots) * 100;
	}

	public async Task<OperationalStatsDTO> GetOperationalStatsAsync()
	{
		var today = DateOnly.FromDateTime(DateTime.UtcNow);
		var startOfDay = today;
		var endOfDay = today.AddDays(1);

		var jobs = await _unitOfWorkCore.Job.Query(j =>
				j.ScheduledDate >= startOfDay &&
				j.ScheduledDate < endOfDay &&
				j.StatusId == GlobalConstants.JOBSTATUS_NOTSTARTED)
			.ToListAsync();

		var locationMap = new Dictionary<Guid, string>
		{
			{ GlobalConstants.JOBLOCATION_SEEDER, "Seeder" },
			{ GlobalConstants.JOBLOCATION_HARVESTER, "Harvester" },
			{ GlobalConstants.JOBLOCATION_TRANSPLANTER, "Transplanter" },
		};

		var jobsToday = jobs
			.GroupBy(j => locationMap.ContainsKey(j.JobLocationTypeId)
				? locationMap[j.JobLocationTypeId]
				: "Unknown")
			.ToDictionary(g => g.Key, g => g.Count());

		return new OperationalStatsDTO { JobsToday = jobsToday };
	}

	public async Task<ResultsStatsDTO> GetResultsStatsAsync(bool usePounds = false)
	{
		// var settings = await _userSettingService.GetUserSettingsByUserIdAsync(userId);
		//bool usePounds = settings?.PreferredWeightUnit == "Pound";

		var startOfToday = DateTime.UtcNow.Date;

		var weightQuery = _unitOfWorkCore.TrayState.Query(ts => ts.HarvestedWeightKG.HasValue && ts.WeightRegistered.HasValue);
		var kgToday = (await weightQuery.Where(ts => ts.WeightRegistered.Value.Date == DateTime.Today).SumAsync(x => x.HarvestedWeightKG)) ?? 0;
		var kgThisMonth = (await weightQuery.Where(ts => ts.WeightRegistered.Value.Year == DateTime.Today.Year && ts.WeightRegistered.Value.Month == DateTime.Today.Month).SumAsync(x => x.HarvestedWeightKG)) ?? 0;
		var kgThisYear = (await weightQuery.Where(ts => ts.WeightRegistered.Value.Year == DateTime.Today.Year).SumAsync(x => x.HarvestedWeightKG)) ?? 0;
		var avgPerDay = (await weightQuery.AverageAsync(x => x.HarvestedWeightKG)) ?? 0;

		var harvestedToday = await weightQuery.Where(ts => ts.WeightRegistered.Value.Date == DateTime.Today).CountAsync();
		var harvestedThisMonth = await weightQuery.Where(ts => ts.WeightRegistered.Value.Year == DateTime.Today.Year && ts.WeightRegistered.Value.Month == DateTime.Today.Month).CountAsync();
		var harvestedThisYear = await weightQuery.Where(ts => ts.WeightRegistered.Value.Year == DateTime.Today.Year).CountAsync();

		var seederQuery = _unitOfWorkCore.TrayState.Query(ts => ts.ArrivedAtSeeder != null);
		var seederToday = await seederQuery.Where(ts => ts.ArrivedAtSeeder.Value.Date == DateTime.Today).CountAsync();
		var seederThisMonth = await seederQuery.Where(ts => ts.ArrivedAtSeeder.Value.Year == DateTime.Today.Year && ts.ArrivedAtSeeder.Value.Month == DateTime.Today.Month).CountAsync();
		var seederThisYear = await seederQuery.Where(ts => ts.ArrivedAtSeeder.Value.Year == DateTime.Today.Year).CountAsync();

		var washedQuery = _unitOfWorkCore.TrayState.Query(ts => ts.ArrivedWashing != null);
		var washedToday = await washedQuery.Where(ts => ts.ArrivedWashing.Value.Date == DateTime.Today).CountAsync();
		var washedThisMonth = await washedQuery.Where(ts => ts.ArrivedWashing.Value.Year == DateTime.Today.Year && ts.ArrivedWashing.Value.Month == DateTime.Today.Month).CountAsync();
		var washedThisYear = await washedQuery.Where(ts => ts.ArrivedWashing.Value.Year == DateTime.Today.Year).CountAsync();

		var paternosterUpQuery = _unitOfWorkCore.TrayState.Query(ts => ts.ArrivedPaternosterUp != null);
		var paternosterUpToday = await paternosterUpQuery.Where(ts => ts.ArrivedPaternosterUp.Value.Date == DateTime.Today).CountAsync();
		var paternosterUpThisMonth = await paternosterUpQuery.Where(ts => ts.ArrivedPaternosterUp.Value.Year == DateTime.Today.Year && ts.ArrivedPaternosterUp.Value.Month == DateTime.Today.Month).CountAsync();
		var paternosterUpThisYear = await paternosterUpQuery.Where(ts => ts.ArrivedPaternosterUp.Value.Year == DateTime.Today.Year).CountAsync();

		var last10Weights = await weightQuery
			.OrderByDescending(ts => ts.WeightRegistered)
			.Take(10)
			.Select(ts => new { ts.HarvestedWeightKG })
			.ToListAsync();

		return new ResultsStatsDTO
		{
			PaternosterUpToday = paternosterUpToday,
			PaternosterUpThisMonth = paternosterUpThisMonth,
			PaternosterUpThisYear = paternosterUpThisYear,
			SeederToday = seederToday,
			SeederThisMonth = seederThisMonth,
			SeederThisYear = seederThisYear,
			WashedToday = washedToday,
			WashedThisMonth = washedThisMonth,
			WashedThisYear = washedThisYear,
			HarvestedToday = harvestedToday,
			HarvestedThisMonth = harvestedThisMonth,
			HarvestedThisYear = harvestedThisYear,
			TotalWeightToday = usePounds ? kgToday * 2.20462f : kgToday,
			TotalWeightThisMonth = usePounds ? kgThisMonth * 2.20462f : kgThisMonth,
			TotalWeightThisYear = usePounds ? kgThisYear * 2.20462f : kgThisYear,
			AverageWeightPerTray = usePounds ? avgPerDay * 2.20462f : avgPerDay,
			WeightUnit = usePounds ? "lbs" : "kg",
			WeightMinusAlert = last10Weights.Count > 0 ? last10Weights.Min(w => w.HarvestedWeightKG) <= -0.04 : false,
		};
	}

	public async Task<List<SystemMessageDTO>> GetSystemMessagesAsync()
	{
		var twentyFourHoursAgo = DateTime.UtcNow.AddHours(-24);
		var messages = await _unitOfWorkAudit.SystemMessage.Query(m => m.AddedDateTime >= twentyFourHoursAgo)
			.Take(100)
			.OrderByDescending(m => m.AddedDateTime)
			.Select(m => new SystemMessageDTO
			{
				Severity = m.Severity,
				Category = m.Category,
				Message = m.Message,
				AddedDateTime = m.AddedDateTime
			})
			.ToListAsync();

		return messages;
	}

}
