namespace VHS.Services.Batches.DTO;

public class JobDTO
{
	public Guid Id { get; set; }
	public Guid? BatchId { get; set; }
    public string? LotReference { get; set; }
    public string? BatchName { get; set; }
	public string Name { get; set; } = string.Empty; //default = batch config name = scheduledatetime ?
	public int OrderOnDay { get; set; } = 1;
	public int TrayCount { get; set; }
	public DateTime ScheduledDate { get; set; }
	public DateTime AddedDateTime { get; set; }
	public Guid JobLocationTypeId { get; set; } = GlobalConstants.JOBLOCATION_SEEDER;

	public Guid StatusId { get; set; } = GlobalConstants.JOBSTATUS_NOTSTARTED;

	public BatchDTO? Batch { get; set; }

	public Guid JobTypeId { get; set; } = Guid.Empty;

	public List<JobTrayDTO> JobTrays { get; set; }

	public bool Paused { get; set; } = false;

	public bool IsEditable
	{
		get
		{
			return StatusId == GlobalConstants.JOBSTATUS_NOTSTARTED || StatusId == GlobalConstants.JOBSTATUS_INPROGRESS;
		}
	}
	public bool IsDeletable
	{
		get
		{
			return StatusId == GlobalConstants.JOBSTATUS_NOTSTARTED;
		}
	}
}
