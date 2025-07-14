//using VHS.Services.Batches.DTO;

//namespace VHS.Services.Farming.DTO;

////current state of the tray
//public class TrayCurrentStateDTO
//{
//    public virtual TrayDTO Tray { get; set; }
            
//    public virtual LayerDTO DestinationLayer { get; set; }

//    public int OrderOnLayer { get; set; } = 0;

//    public virtual BatchDTO Batch { get; set; }

//    public Guid CurrentPhaseId { get; set; } = GlobalConstants.TRAYPHASE_EMPTY;
//    public DateTime? SeededDateTimeUTC { get; set; }

//    public string ProduceType { get; set; } = string.Empty;

//    public bool IsFinishedGrowing
//    {
//        get
//        {
//            return CurrentPhaseId == GlobalConstants.TRAYPHASE_FULLYGROWN;
//        }
//    }

//    public bool IsFinishedGerminating
//    {
//        get
//        {
//            return CurrentPhaseId == GlobalConstants.TRAYPHASE_FULLYGERMINATED;
//        }
//    }

//    public bool IsFinishedPropagating
//    {
//        get
//        {
//            return CurrentPhaseId == GlobalConstants.TRAYPHASE_FULLYPROPAGATED;
//        }
//    }

//    public bool IsEmpty
//    {
//        get
//        {
//            return CurrentPhaseId == GlobalConstants.TRAYPHASE_EMPTY;
//        }
//    }

//    public bool IsGrowing
//    {
//        get
//        {
//            return CurrentPhaseId == GlobalConstants.TRAYPHASE_GROWING;
//        }
//    }

//    public int DaysGrowing(DateTime currentDateTime)
//    {
//        return SeededDateTimeUTC.HasValue ? (int)(currentDateTime - SeededDateTimeUTC.Value).TotalDays : 0;
//    }

//    public void AddOccupiedDay(DateTime currentDateTime)
//    {            
//        if (CurrentPhaseId == GlobalConstants.TRAYPHASE_GROWING)
//        {
//            if (DaysGrowing(currentDateTime) >= this.Batch.BatchPlan.Recipe.GrowDays)
//            {
//                CurrentPhaseId = GlobalConstants.TRAYPHASE_FULLYGROWN;
//            }
//        }
//        if (CurrentPhaseId == GlobalConstants.TRAYPHASE_GERMINATING)
//        {
//            if (DaysGrowing(currentDateTime) >= this.Batch.BatchPlan.Recipe.GerminationDays)
//            {
//                CurrentPhaseId = GlobalConstants.TRAYPHASE_FULLYGERMINATED;
//            }
//        }
//        if (CurrentPhaseId == GlobalConstants.TRAYPHASE_PROPAGATING)
//        {
//            if (DaysGrowing(currentDateTime) >= this.Batch.BatchPlan.Recipe.GerminationDays)
//            {
//                CurrentPhaseId = GlobalConstants.TRAYPHASE_FULLYPROPAGATED;
//            }
//        }
//    }
//}
