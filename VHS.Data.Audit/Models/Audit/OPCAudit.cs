namespace VHS.Data.Models.Audit;

public class OPCAudit
{
	public Guid Id { get; set; }
	public Guid FarmId { get; set; }
	public int EventId { get; set; }

	public string? TrayTag { get; set; } = null;

	public string MessageInput { get; set; } = string.Empty;
	public string MessageOutput { get; set; } = string.Empty;

	public DateTime ReceiveDateTime { get; set; }
	public DateTime MessageInputDateTime { get; set; }
	public DateTime MessageOutputDateTime { get; set; }
}
