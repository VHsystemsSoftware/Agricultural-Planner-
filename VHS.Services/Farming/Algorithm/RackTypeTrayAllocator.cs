//using System;
//using System.Collections.Generic;
//using System.Linq;
//using VHS.Services.Farming.DTO;
//using VHS.Services.Batches.DTO;
//using VHS.Services.Common;
//using VHS.Services.Farming.Constants;

//namespace VHS.Services.Farming
//{
//    public class RackTypeTrayAllocator
//    {
//        public void AllocateTraysByRackType(FarmDTO farm, List<BatchDTO> batches, DateTime currentSimulatedDate)
//        {
//            // Process phases in order: seeding → germination → growing.
//            //AllocateToGerminationRacks(farm, batches, currentSimulatedDate);
//            //AllocateToPropagationRacks(farm, batches, currentSimulatedDate);
//            //AllocateToGrowingRacks(farm, batches, currentSimulatedDate);
//        }

//        //private void AllocateToGerminationRacks(FarmDTO farm, List<BatchDTO> batches, DateTime currentSimulatedDate)
//        //{
//        //    foreach (var batch in batches)
//        //    {
//        //        if (batch.Trays.All(tray => tray.IsFinishedGerminating))
//        //            continue;

//        //        foreach (var tray in batch.Trays.Where(tray => tray.IsEmpty && !tray.IsFinishedGerminating))
//        //        {
//        //            var availableRack = farm.GerminationRacks.FirstOrDefault(rack => rack.HasRoom);
//        //            if (availableRack == null)
//        //                break;

//        //            var freeLayer = availableRack.Layers.FirstOrDefault(layer => layer.HasRoom);
//        //            if (freeLayer == null)
//        //                continue;

//        //            tray.DestinationLayer = freeLayer;
//        //            tray.OrderOnLayer = freeLayer.Trays.Count + 1;
//        //            tray.CurrentPhaseId = GlobalConstants.TRAYPHASE_GERMINATING;
//        //            tray.SeededDateTimeUTC = currentSimulatedDate;
//        //            if (string.IsNullOrEmpty(tray.ProduceType))
//        //                tray.ProduceType = batch.ProduceType; // Ensure produce type is assigned.
//        //            freeLayer.Trays.Add(tray);
//        //            availableRack.AddOccupiedDays(currentSimulatedDate);

//        //        }
//        //    }
//        //}

//        //private void AllocateToPropagationRacks(FarmDTO farm, List<BatchDTO> batches, DateTime currentSimulatedDate)
//        //{
//        //    // Propagation racks: use configuration from RackConstant.SK2a (9 layers, 54 trays per layer).
//        //    foreach (var batch in batches)
//        //    {
//        //        if (batch.Trays.All(tray => tray.IsFinishedPropagating))
//        //            continue;

//        //        foreach (var tray in batch.Trays.Where(tray => tray.IsEmpty && !tray.IsFinishedPropagating))
//        //        {
//        //            var availableRack = farm.PropagationRacks.FirstOrDefault(rack => rack.HasRoom);
//        //            if (availableRack == null)
//        //                break;

//        //            var freeLayer = availableRack.Layers.FirstOrDefault(layer => layer.HasRoom);
//        //            if (freeLayer == null)
//        //                continue;

//        //            tray.DestinationLayer = freeLayer;
//        //            tray.OrderOnLayer = freeLayer.Trays.Count + 1;
//        //            tray.CurrentPhaseId = GlobalConstants.TRAYPHASE_PROPAGATING;
//        //            tray.SeededDateTimeUTC = currentSimulatedDate;
//        //            freeLayer.Trays.Add(tray);
//        //            availableRack.AddOccupiedDays(currentSimulatedDate);
//        //        }
//        //    }
//        //}

//        //private void AllocateToGrowingRacks(FarmDTO farm, List<BatchDTO> batches, DateTime currentSimulatedDate)
//        //{
//        //    // Growing racks: these can be of various types.
//        //    // Allocation is performed from the available layers; reserve the last layer (transportation layer) if more than one is available.
//        //    foreach (var batch in batches)
//        //    {
//        //        if (batch.Trays.All(tray => tray.IsFinishedGrowing))
//        //            continue;

//        //        foreach (var tray in batch.Trays.Where(tray => tray.IsEmpty && !tray.IsFinishedGrowing))
//        //        {
//        //            var availableRack = farm.GrowRacks.FirstOrDefault(rack => rack.HasRoom);
//        //            if (availableRack == null)
//        //                break;

//        //            // Get available layers without ordering by a non-existent property.
//        //            var freeLayers = availableRack.Layers
//        //                                          .Where(layer => layer.HasRoom)
//        //                                          .ToList();

//        //            // Reserve the last layer (transportation layer) if there is more than one available.
//        //            if (freeLayers.Count > 1)
//        //                freeLayers.Remove(freeLayers.Last());

//        //            var freeLayer = freeLayers.FirstOrDefault();
//        //            if (freeLayer == null)
//        //                continue;

//        //            tray.DestinationLayer = freeLayer;
//        //            tray.OrderOnLayer = freeLayer.Trays.Count + 1;
//        //            tray.CurrentPhaseId = GlobalConstants.TRAYPHASE_GROWING;
//        //            tray.SeededDateTimeUTC = currentSimulatedDate;
//        //            freeLayer.Trays.Add(tray);
//        //            availableRack.AddOccupiedDays(currentSimulatedDate);
//        //        }
//        //    }
//        //}
//    }
//}
