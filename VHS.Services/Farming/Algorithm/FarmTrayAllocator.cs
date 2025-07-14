//using System;
//using System.Collections.Generic;
//using System.Linq;
//using VHS.Services.Farming.DTO;
//using VHS.Services.Batches.DTO;
//using VHS.Services.Common;
//using VHS.Services.Farming.Constants;

//namespace VHS.Services.Farming.Algorithm
//{
//    public class FarmTrayAllocator
//    {
//        private readonly DateTime _currentTime;

//        public FarmTrayAllocator()
//        {
//            _currentTime = DateTime.UtcNow;
//        }

//        /// <summary>
//        /// Processes tray movement through phases: seeding → germination → growing → harvesting → washing → (optional stacker).
//        /// </summary>
//        public DayAllocation AllocateTrays(FarmDTO farm, List<BatchDTO> batches, DateTime currentDate)
//        {
//            int totalTraysMovedToday = 0;
//            int seededToday = 0;
//            int germinatingToday = 0;
//            int growingToday = 0;
//            int harvestedToday = 0;
//            int washedToday = 0;

//            var detailedAllocations = new List<DetailedAllocation>();

//            // 1. Seeding to Germination: Trays are seeded in batches of 4.
//            foreach (var batch in batches.Where(b => b.StatusId == GlobalConstants.BATCHSTATUS_PENDING))
//            {
//                foreach (var tray in batch.Trays.Where(t => t.CurrentPhaseId == GlobalConstants.TRAYPHASE_SEEDED && t.IsEmpty).Take(4))
//                {
//                    AllocateToGermination(farm, new List<TrayCurrentStateDTO> { tray }, ref totalTraysMovedToday, detailedAllocations);
//                    seededToday++;
//                    tray.CurrentPhaseId = GlobalConstants.TRAYPHASE_GERMINATING; // Move tray to germination phase
//                }

//                // Update batch status if all trays are in germination
//                if (batch.Trays.All(t => t.CurrentPhaseId == GlobalConstants.TRAYPHASE_GERMINATING))
//                {
//                    batch.StatusId = GlobalConstants.BATCHSTATUS_INPROGRESS;
//                }
//            }

//            // 2. Germination to Growing
//            foreach (var batch in batches.Where(b => b.StatusId == GlobalConstants.BATCHSTATUS_INPROGRESS))
//            {
//                foreach (var tray in batch.Trays.Where(t => t.CurrentPhaseId == GlobalConstants.TRAYPHASE_GERMINATING && t.IsFinishedGerminating))
//                {
//                    AllocateToGrowing(farm, new List<TrayCurrentStateDTO> { tray }, ref totalTraysMovedToday, detailedAllocations);
//                    germinatingToday++;
//                    tray.CurrentPhaseId = GlobalConstants.TRAYPHASE_GROWING; // Move tray to growing phase
//                }

//                // Update batch status if all trays are in growing phase
//                if (batch.Trays.All(t => t.CurrentPhaseId == GlobalConstants.TRAYPHASE_GROWING))
//                {
//                    batch.StatusId = GlobalConstants.BATCHSTATUS_INPROGRESS;
//                }
//            }

//            // 3. Growing to Harvesting
//            foreach (var batch in batches.Where(b => b.StatusId == GlobalConstants.BATCHSTATUS_INPROGRESS))
//            {
//                foreach (var tray in batch.Trays.Where(t => t.CurrentPhaseId == GlobalConstants.TRAYPHASE_GROWING && t.IsFinishedGrowing))
//                {
//                    AllocateToHarvesting(farm, new List<TrayCurrentStateDTO> { tray }, ref totalTraysMovedToday, detailedAllocations);
//                    growingToday++;
//                    tray.CurrentPhaseId = GlobalConstants.TRAYPHASE_HARVESTED; // Move tray to harvested phase
//                }

//                // Update batch status if all trays are harvested
//                if (batch.Trays.All(t => t.CurrentPhaseId == GlobalConstants.TRAYPHASE_HARVESTED))
//                {
//                    batch.StatusId = GlobalConstants.BATCHSTATUS_HARVESTED;
//                }
//            }

//            // 4. Harvesting to Washing
//            foreach (var batch in batches.Where(b => b.StatusId == GlobalConstants.BATCHSTATUS_HARVESTED))
//            {
//                foreach (var tray in batch.Trays.Where(t => t.CurrentPhaseId == GlobalConstants.TRAYPHASE_HARVESTED && t.IsEmpty))
//                {
//                    AllocateToWashing(farm, new List<TrayCurrentStateDTO> { tray }, ref totalTraysMovedToday, detailedAllocations);
//                    harvestedToday++;
//                    tray.CurrentPhaseId = GlobalConstants.TRAYPHASE_WASHED; // Move tray to washed phase
//                }

//                // Update batch status if all trays are washed
//                if (batch.Trays.All(t => t.CurrentPhaseId == GlobalConstants.TRAYPHASE_WASHED))
//                {
//                    batch.StatusId = GlobalConstants.BATCHSTATUS_COMPLETED;
//                }
//            }

//            // 5. Allocate to stacker if space
//            if (totalTraysMovedToday < TrayConstant.MaxDailyTrays)
//            {
//                AllocateToStacker(farm, ref totalTraysMovedToday);
//            }

//            ReportDailyOutput(farm);

//            return new DayAllocation
//            {
//                Seeding = seededToday,
//                Germination = germinatingToday,
//                Growing = growingToday,
//                Harvest = harvestedToday,
//                //Washed = washedToday.
//                TrayMovements = totalTraysMovedToday,
//                DetailedAllocations = detailedAllocations
//            };
//        }


//        private void AllocateToGermination(FarmDTO farm, List<TrayCurrentStateDTO> trays, ref int movedCount, List<DetailedAllocation> detailedAllocations)
//        {
//            foreach (var rack in farm.GerminationRacks.Where(r => r.HasRoom))
//            {
//                foreach (var tray in trays)
//                {
//                    if (movedCount >= TrayConstant.MaxDailyTrays)
//                        return;
//                    var freeLayer = rack.Layers.FirstOrDefault(l => l.HasRoom);
//                    if (freeLayer != null)
//                    {
//                        var targetSlot = freeLayer.Trays.FirstOrDefault(t => t.IsEmpty);
//                        if (targetSlot != null)
//                        {
//                            targetSlot.CurrentPhaseId = GlobalConstants.TRAYPHASE_GERMINATING;
//                            targetSlot.SeededDateTimeUTC = _currentTime;

//                            targetSlot.ProduceType = tray.ProduceType;
//                            movedCount++;

//                            detailedAllocations.Add(new DetailedAllocation
//                            {
//                                ProduceType = tray.ProduceType,
//                                RackType = "Germination",
//                                TrayCount = 1,
//                                WasteTrays = 0,
//                                Phase = "Germination"
//                            });

//                            // Mark the slot as filled (if needed, ensure IsEmpty becomes false)
//                            //targetSlot.IsEmpty = false;
//                        }
//                    }
//                }
//            }
//        }

//        private void AllocateToGrowing(FarmDTO farm, List<TrayCurrentStateDTO> trays, ref int movedCount, List<DetailedAllocation> detailedAllocations)
//        {
//            // Allocate trays to growing racks.
//            foreach (var rack in farm.GrowRacks.Where(r => r.HasRoom))
//            {
//                // Get available layers without ordering by a non-existent LayerIndex property.
//                var freeLayers = rack.Layers
//                                     .Where(l => l.HasRoom)
//                                     .OrderBy(l => l.LayerNumber)
//                                     .ToList();

//                // Reserve the lowest (last) layer if there is more than one available.
//                if (freeLayers.Count > 1)
//                    freeLayers.Remove(freeLayers.Last());

//                foreach (var tray in trays)
//                {
//                    if (movedCount >= TrayConstant.MaxDailyTrays)
//                        return;

//                    var freeLayer = freeLayers.FirstOrDefault();
//                    if (freeLayer != null)
//                    {
//                        var targetSlot = freeLayer.Trays.FirstOrDefault(t => t.IsEmpty);
//                        if (targetSlot != null)
//                        {
//                            targetSlot.CurrentPhaseId = GlobalConstants.TRAYPHASE_GROWING;
//                            targetSlot.SeededDateTimeUTC = _currentTime;
//                            movedCount++;
//                            detailedAllocations.Add(new DetailedAllocation
//                            {
//                                ProduceType = tray.ProduceType,
//                                RackType = "Grow",
//                                TrayCount = 1,
//                                WasteTrays = 0,
//                                Phase = "Growing"
//                            });

//                            //targetSlot.IsEmpty = false;
//                        }
//                    }
//                }
//            }
//        }

//        private void AllocateToHarvesting(FarmDTO farm, List<TrayCurrentStateDTO> trays, ref int movedCount, List<DetailedAllocation> detailedAllocations)
//        {
//            // Harvesting: Only one rack with one layer (assumed provided in _farm.HarvestingRacks).
//            foreach (var rack in farm.HarvestingRacks)
//            {
//                foreach (var tray in trays)
//                {
//                    if (movedCount >= TrayConstant.MaxDailyTrays)
//                        return;

//                    var freeLayer = rack.Layers.FirstOrDefault(l => l.HasRoom);
//                    if (freeLayer != null)
//                    {
//                        var targetSlot = freeLayer.Trays.FirstOrDefault(t => t.IsEmpty);
//                        if (targetSlot != null)
//                        {
//                            targetSlot.CurrentPhaseId = GlobalConstants.TRAYPHASE_HARVESTED;
//                            movedCount++;
//                            detailedAllocations.Add(new DetailedAllocation
//                            {
//                                ProduceType = tray.ProduceType,
//                                RackType = "Harvesting",
//                                TrayCount = 1,
//                                WasteTrays = 0,
//                                Phase = "Harvesting"
//                            });

//                            //targetSlot.IsEmpty = false;
//                        }
//                    }
//                }
//            }
//        }

//        private void AllocateToWashing(FarmDTO farm, List<TrayCurrentStateDTO> trays, ref int movedCount, List<DetailedAllocation> detailedAllocations)
//        {
//            // Washing: Only one rack with one layer (assumed provided in _farm.WashingRacks).
//            foreach (var rack in farm.WashingRacks)
//            {
//                foreach (var tray in trays)
//                {
//                    if (movedCount >= TrayConstant.MaxDailyTrays)
//                        return;

//                    var freeLayer = rack.Layers.FirstOrDefault(l => l.HasRoom);
//                    if (freeLayer != null)
//                    {
//                        var targetSlot = freeLayer.Trays.FirstOrDefault(t => t.IsEmpty);
//                        if (targetSlot != null)
//                        {
//                            targetSlot.CurrentPhaseId = GlobalConstants.TRAYPHASE_WASHED;
//                            movedCount++;
//                            detailedAllocations.Add(new DetailedAllocation
//                            {
//                                ProduceType = tray.ProduceType,
//                                RackType = "Washing",
//                                TrayCount = 1,
//                                WasteTrays = 0,
//                                Phase = "Washing"
//                            });

//                            //targetSlot.IsEmpty = false;
//                        }
//                    }
//                }
//            }
//        }

//        private void AllocateToStacker(FarmDTO farm, ref int movedCount)
//        {
//            // Allocate any remaining trays to the stacker (up to 500 trays capacity).
//            // Implement your logic here based on your stacker DTOs.
//            // Example:
//            // var stacker = _farm.Stackers.FirstOrDefault();
//            // if (stacker != null && movedCount < TrayConstant.MaxDailyTrays)
//            // {
//            //     // Add trays to the stacker until capacity is reached.
//            //     movedCount++;
//            // }
//            Console.WriteLine("Allocating remaining trays to stacker...");
//            // TODO: Implement detailed stacker allocation logic if needed.
//        }

//        private void ReportDailyOutput(FarmDTO farm)
//        {
//            int dailyOutput = farm.GerminationRacks.Sum(rack => rack.OccupiedLayers) +
//                              farm.PropagationRacks.Sum(rack => rack.OccupiedLayers) +
//                              farm.GrowRacks.Sum(rack => rack.OccupiedLayers);
//            Console.WriteLine($"Daily trays processed: {dailyOutput}");
//            // Optionally, include reporting on the days per rack based on germination and growing days.
//        }
//    }
//}
