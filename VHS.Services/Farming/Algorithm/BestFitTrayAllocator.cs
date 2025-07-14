//using System;
//using System.Collections.Generic;
//using System.Linq;
//using VHS.Services.Common;
//using VHS.Services.Farming.DTO;
//using VHS.Services.Farming.Constants;
//using VHS.Data.Models.Farming;

//namespace VHS.Services.Farming.Algorithm
//{
//    public class BestFitTrayAllocator
//    {
//        /// <summary>
//        /// Allocates trays for each product type based on daily demand.
//        /// Uses each grow rack’s TrayDepth property to determine available capacity.
//        /// </summary>
//        public List<BestFitResult> AllocateTrays(FarmDTO farm, List<DemandGrowthDTO> demandGrowthInfos)
//        {
//            var bestFitResults = new List<BestFitResult>();
//            foreach (var demand in demandGrowthInfos)
//            {
//                Console.WriteLine($"Allocating trays for {demand.ProductType}...");

//                decimal dailyDemand = (decimal)demand.DailyDemand;
//                decimal plusMinAdjustment = decimal.Round(dailyDemand * (decimal)demand.PlusMinPercentage / 100m, 0);
//                int traysRequired = (int)decimal.Round(dailyDemand - plusMinAdjustment, 0);

//                var availableLayers = GetAvailableLayerInfos(farm);
//                var bestCombination = new Dictionary<int, int>();
//                int currentBestSum = 0;
//                FindBestCombination(availableLayers, 0, new Dictionary<int, int>(), 0, ref bestCombination, ref currentBestSum, traysRequired);

//                // If no combination was found, fall back to a default allocation (all zeroes)
//                if (bestCombination.Count == 0)
//                {
//                    foreach (var kv in availableLayers)
//                    {
//                        bestCombination[kv.Key] = 0;
//                    }
//                }

//                var allocationDetail = new BestFitResult { ProduceType = demand.ProductType.ToString() };
//                foreach (var allocation in bestCombination)
//                {
//                    int layerCapacity = allocation.Key;
//                    int countUsed = allocation.Value;
//                    int totalAllocated = layerCapacity * countUsed;
//                    int waste = (totalAllocated - traysRequired) > 0 ? (totalAllocated - traysRequired) : 0;
//                    // Enforce a limit of max 10 wasted trays per allocation if exceeding.
//                    if (waste > 10)
//                        Console.WriteLine($"Warning: Waste for layer capacity {layerCapacity} exceeds the 10 tray limit.");

//                    // Adjust waste if negative.
//                    waste = waste < 0 ? 0 : waste;
//                    allocationDetail.LayerAllocations[layerCapacity] = (Allocated: totalAllocated, Waste: waste);
//                    Console.WriteLine($"[Best-Fit] Produce: {demand.ProductType} - Layer capacity {layerCapacity}: {countUsed} layers used, total {totalAllocated} trays, waste {waste}");
//                }
//                bestFitResults.Add(allocationDetail);

//                // Allocate trays to grow racks based on bestCombination.
//                AllocateTraysToGrowRacks(bestCombination, farm);
//            }
//            return bestFitResults;
//        }

//        /// <summary>
//        /// Allocates trays to the grow racks using the best-fit allocation plan.
//        /// Layers are assumed to be in the correct order (top-to-bottom) and the lowest (transportation) layer is reserved if possible.
//        /// </summary>
//        private void AllocateTraysToGrowRacks(Dictionary<int, int> allocationPlan, FarmDTO farm)
//        {
//            foreach (var rack in farm.GrowRacks)
//            {
//                int rackCapacity = rack.TrayDepth;
//                var freeLayers = rack.Layers.Where(layer => layer.HasRoom).ToList();

//                if (freeLayers.Count > 1)
//                    freeLayers.Remove(freeLayers.Last()); // Reserve transportation layer

//                foreach (var plan in allocationPlan)
//                {
//                    int layerCapacity = plan.Key;
//                    int traysToAllocate = plan.Value; // Represents the number of full layers to allocate

//                    foreach (var layer in freeLayers)
//                    {
//                        int spaceAvailable = layer.Trays.Count(t => t.IsEmpty);
//                        int traysForLayer = Math.Min(traysToAllocate, spaceAvailable);

//                        for (int i = 0; i < traysForLayer; i++)
//                        {
//                            var tray = new TrayCurrentStateDTO
//                            {
//                                CurrentPhaseId = GlobalConstants.TRAYPHASE_SEEDED,
//                                SeededDateTimeUTC = DateTime.UtcNow,
//                                DestinationLayer = layer,
//                                ProduceType = string.Empty
//                            };
//                            layer.Trays.Add(tray);
//                        }
//                        traysToAllocate -= traysForLayer;
//                        if (traysToAllocate <= 0)
//                            break;
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// Retrieves a list of available layer info from grow racks.
//        /// The key is the rack’s tray capacity and the value is the number of available layers.
//        /// </summary>
//        private List<KeyValuePair<int, int>> GetAvailableLayerInfos(FarmDTO farm)
//        {
//            return farm.GrowRacks
//                        .SelectMany(rack => rack.Layers
//                            .Where(layer => layer.HasRoom)
//                            .Select(layer => new KeyValuePair<int, int>(rack.TrayDepth, rack.Layers.Count(l => l.HasRoom))))
//                        .ToList();
//        }

//        /// <summary>
//        /// Recursively finds the best combination of available layer capacities that approaches the target tray count.
//        /// Now accepts combinations that overshoot the target and chooses the one with the smallest waste.
//        /// </summary>
//        private static void FindBestCombination(
//            List<KeyValuePair<int, int>> availableLayers,
//            int index,
//            Dictionary<int, int> currentAllocation,
//            int currentSum,
//            ref Dictionary<int, int> bestAllocation,
//            ref int bestSum,
//            int target)
//        {
//            // If we reached or exceeded the target, record this combination if it has less waste.
//            if (currentSum >= target)
//            {
//                int currentWaste = currentSum - target;
//                int bestWaste = bestSum >= target ? bestSum - target : int.MaxValue;
//                if (currentWaste < bestWaste)
//                {
//                    bestSum = currentSum;
//                    bestAllocation = new Dictionary<int, int>(currentAllocation);
//                }
//                return; // No need to add more, as extra layers would only increase waste.
//            }

//            if (index >= availableLayers.Count)
//                return;

//            var (layerCapacity, maxCount) = availableLayers[index];
//            for (int count = 0; count <= maxCount; count++)
//            {
//                currentAllocation[layerCapacity] = count;
//                FindBestCombination(
//                    availableLayers,
//                    index + 1,
//                    currentAllocation,
//                    currentSum + layerCapacity * count,
//                    ref bestAllocation,
//                    ref bestSum,
//                    target);
//            }
//            currentAllocation.Remove(layerCapacity);
//        }
//    }
//}
