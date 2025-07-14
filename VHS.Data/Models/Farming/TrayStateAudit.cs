using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHS.Data.Core.Models;

public class TrayStateAudit
{
	[Key]
	public Guid Id { get; set; }
	public Guid TrayStateId { get; set; }
	public Guid OPCAuditId { get; set; }

	public virtual TrayState TrayState { get; set; }

	public DateTime AddedDateTime { get; set; }
	
}
