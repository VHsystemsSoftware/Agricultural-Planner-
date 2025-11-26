//using System;
//using System.Collections.Generic;

//namespace VHS.Services.Farming.Algorithm
//{
//    public class FarmAllocationResult
//    {
//        public FarmAllocationResult()
//        {
//            AllocationDetails = new List<AllocationDetail>();
//        }

//        public DateTime AllocationDate { get; set; }
//        public int TotalTraysAllocated { get; set; }
//        public int TotalTraysFailed { get; set; }
//        public List<AllocationDetail> AllocationDetails { get; set; }

//        public void AddDetail(string productType, string rackType, int traysAllocated, int traysFailed)
//        {
//            AllocationDetails.Add(new AllocationDetail
//            {
//                ProductType = productType,
//                RackType = rackType,
//                TraysAllocated = traysAllocated,
//                TraysFailed = traysFailed,
//                AllocationDate = DateTime.UtcNow
//            });

//            TotalTraysAllocated += traysAllocated;
//            TotalTraysFailed += traysFailed;
//        }
//    }

//    public class AllocationDetail
//    {
//        public string ProductType { get; set; }
//        public string RackType { get; set; }
//        public int TraysAllocated { get; set; }
//        public int TraysFailed { get; set; }
//        public DateTime AllocationDate { get; set; }
//    }
//}
