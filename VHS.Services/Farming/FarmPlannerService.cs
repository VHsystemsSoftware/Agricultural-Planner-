//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using VHS.Services.Farming.Algorithm;
//using VHS.Services.Farming.Constants;
//using VHS.Services.Farming.DTO;
//using VHS.Services.Produce.DTO;
//using VHS.Services.Batches.DTO;
//using VHS.Services.Common;

//namespace VHS.Services.Farming
//{
//    public class BestFitResult
//    {
//        public string ProduceType { get; set; } = string.Empty;
//        public Dictionary<int, (int Allocated, int Waste)> LayerAllocations { get; set; } = new();
//    }

//    public class FarmAllocationPlan
//    {
//        public bool Success { get; set; }
//        public string? ErrorMessage { get; set; }
//        public List<DayAllocation> DailyAllocations { get; set; } = new();
//        public int TotalHarvestPerDay { get; set; }
//        public int MaxFarmCapacity { get; set; }
//        public List<DetailedAllocation> DetailedAllocations { get; set; } = new();
//        public List<BestFitResult> BestFitResults { get; set; } = new();
//        public int TotalTraysPlanned { get; set; }
//    }

//    public class TrayMovementResult
//    {
//        public int Seeded { get; set; }
//        public int Germinating { get; set; }
//        public int Growing { get; set; }
//        public int Harvested { get; set; }
//        public int TrayMovements { get; set; }
//        public List<DetailedAllocation> DetailedAllocations { get; set; } = new();
//    }

//    public class DayAllocation
//    {
//        public int Day { get; set; }
//        public int Seeding { get; set; }
//        public int Germination { get; set; }
//        public int Growing { get; set; }
//        public int Harvest { get; set; }
//        public int TrayMovements { get; set; }
//        public List<DetailedAllocation> DetailedAllocations { get; set; } = new();
//    }

//    public class FarmPlanRequest
//    {
//        public List<ProductCategoryBatchSizeDTO> BatchSizes { get; set; } = new();
//        public int TotalDays { get; set; }
//        public int TotalTraysAvailable { get; set; }
//        public DateTime StartDate { get; set; }
//    }

//    public class DetailedAllocation
//    {
//        public string ProduceType { get; set; } = string.Empty;
//        public string RackType { get; set; } = string.Empty;
//        public int TrayCount { get; set; }
//        public int WasteTrays { get; set; }
//        public string Phase { get; set; } = string.Empty;
//    }

//    public interface IFarmPlannerService
//    {
//        Task<FarmAllocationPlan> PlanFarmAsync(
//            FarmDTO farm,
//            List<ProductCategoryBatchSizeDTO> batchSizes,
//            int totalDays,
//            int totalTraysAvailable,
//            DateTime startDate);
//    }

//    public class FarmPlannerService : IFarmPlannerService
//    {
//        private readonly BestFitTrayAllocator _bestFit;
//        private readonly FarmTrayAllocator _farmTray;
//        private readonly RackTypeTrayAllocator _rackType;

//        public FarmPlannerService(
//            BestFitTrayAllocator bestFit,
//            FarmTrayAllocator farmTray,
//            RackTypeTrayAllocator rackType)
//        {
//            _bestFit = bestFit;
//            _farmTray = farmTray;
//            _rackType = rackType;
//        }

//        public async Task<FarmAllocationPlan> PlanFarmAsync(
//            FarmDTO farm,
//            List<ProductCategoryBatchSizeDTO> batchSizes,
//            int totalDays,
//            int totalTraysAvailable,
//            DateTime startDate)
//        {
//            var plan = new FarmAllocationPlan();
//            try
//            {
//                foreach (var product in batchSizes)
//                {
//                    if (product.ProductCategory.Id == GlobalConstants.PRODUCTCATEGORY_LETTUCE)
//                    {
//                        int neededPropTrays = (int)Math.Ceiling((double)product.TraysPerDay / 24);
//                        int neededPropLayers = (int)Math.Ceiling((double)neededPropTrays / RackConstant.SK2aDepth);
//                        int availablePropLayers = farm.PropagationRacks.Sum(r => r.Layers.Count);
//                        if (neededPropLayers > availablePropLayers)
//                            throw new Exception($"Insufficient propagation capacity for {product.ProductCategory.Name}");
//                    }
//                    else if (product.ProductCategory.Id == GlobalConstants.PRODUCTCATEGORY_PETITEGREENS ||
//                             product.ProductCategory.Id == GlobalConstants.PRODUCTCATEGORY_MICROGREENS)
//                    {
//                        int availableGrowLayers = farm.GrowRacks.Sum(r => r.Layers.Count);
//                        int requiredGrowLayers = 0;
//                        if (farm.GrowRacks.Any())
//                        {
//                            int sampleTrayDepth = farm.GrowRacks.First().TrayDepth;
//                            requiredGrowLayers = (int)Math.Ceiling((double)product.TraysPerDay / sampleTrayDepth);
//                        }
//                        if (requiredGrowLayers > availableGrowLayers)
//                            throw new Exception($"Insufficient growing rack capacity for {product.ProductCategory.Name}");

//                        int germinationTrayDepth = farm.GerminationRacks.Any() ? farm.GerminationRacks.First().TrayDepth : 27;
//                        int requiredGerminationLayers = (int)Math.Ceiling((double)product.TraysPerDay / germinationTrayDepth);
//                        int availableGerminationLayers = farm.GerminationRacks.Sum(r => r.Layers.Count);
//                        if (requiredGerminationLayers > availableGerminationLayers)
//                            throw new Exception($"Insufficient germination capacity for {product.ProductCategory.Name}");
//                    }
//                }

//                // Run best-fit allocation to get detailed tray per rack and produce type info
//                var demandGrowthList = batchSizes.Select(b => new DemandGrowthDTO
//                {
//                    ProductType = b.ProductCategory.Id,
//                    DailyDemand = b.TraysPerDay,
//                    PlusMinPercentage = (double)b.PlusMinPercentage
//                }).ToList();

//                var bestFitResults = _bestFit.AllocateTrays(farm, demandGrowthList);

//                // Build a day-by-day schedule
//                for (int day = 1; day <= totalDays; day++)
//                {
//                    var currentDate = startDate.AddDays(day - 1);
//                    var batchDTOs = batchSizes.Select(b => new BatchDTO
//                    {
//                        FarmId = farm.Id,
//                        StatusId = GlobalConstants.BATCHSTATUS_PENDING,
//                        ProduceType = b.ProductCategory.Name,
//                        Trays = Enumerable.Range(0, b.TraysPerDay)
//                            .Select(trayIndex => new TrayCurrentStateDTO
//                            {
//                                CurrentPhaseId = GlobalConstants.TRAYPHASE_EMPTY,
//                                ProduceType = b.ProductCategory.Name
//                            })
//                            .ToList()
//                    }).ToList();

//                    _rackType.AllocateTraysByRackType(farm, batchDTOs, currentDate);

//                    var result = _farmTray.AllocateTrays(farm, batchDTOs, currentDate);

//                    plan.DailyAllocations.Add(new DayAllocation
//                    {
//                        Day = day,
//                        Seeding = result.Seeding,
//                        Germination = result.Germination,
//                        Growing = result.Growing,
//                        Harvest = result.Harvest,
//                        TrayMovements = result.TrayMovements,
//                        DetailedAllocations = result.DetailedAllocations
//                    });

//                    plan.DetailedAllocations.AddRange(result.DetailedAllocations);
//                }

//                plan.BestFitResults = bestFitResults;
//                plan.TotalHarvestPerDay = batchSizes.Sum(b => b.TraysPerDay);
//                plan.MaxFarmCapacity = farm.GrowRacks.Sum(r => r.Layers.Count * r.TrayDepth);
//                // Calculate TotalTraysPlanned as the sum of daily allocations across all phases.
//                plan.TotalTraysPlanned = plan.DailyAllocations.Sum(d => d.Seeding + d.Germination + d.Growing + d.Harvest);
//                plan.Success = true;
//            }
//            catch (Exception ex)
//            {
//                plan.Success = false;
//                plan.ErrorMessage = ex.Message;
//            }

//            return await Task.FromResult(plan);
//        }
//    }
//}
