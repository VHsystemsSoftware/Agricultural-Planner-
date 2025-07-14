using System.Security.Cryptography.X509Certificates;
using VHS.Data.Audit.Infrastructure;
using VHS.Data.Models.Audit;

namespace VHS.Services;

public interface IOPCAuditService
{
	Task<OPCAudit> ReceiveOPCAuditAsync(Guid farmId, int eventId, string trayTag);
	Task<OPCAudit> SendOPCAuditAsync(Guid id);
	Task<OPCAudit> UpdateOPCAuditInputMessageAsync(Guid id, string messageOutput);
	Task<OPCAudit> UpdateOPCAuditOutputMessageAsync(Guid id, string messageOutput);
	Task DeleteOPCAudit(Guid id);
}

public class OPCAuditService: IOPCAuditService
{
	private readonly IUnitOfWorkAudit _unitOfWork;


	public OPCAuditService(IUnitOfWorkAudit unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<OPCAudit> ReceiveOPCAuditAsync(Guid farmId, int eventId, string trayTag)
	{
		var audit = new OPCAudit()
		{
			Id = Guid.NewGuid(),
			ReceiveDateTime = DateTime.UtcNow,
			FarmId = farmId,
			EventId = eventId,
			TrayTag= trayTag,
		};

		await _unitOfWork.OPCAudit.AddAsync(audit);
		await _unitOfWork.SaveChangesAsync();

		return audit;
	}

	public async Task<OPCAudit> SendOPCAuditAsync(Guid id)
	{
		var audit = await _unitOfWork.OPCAudit.GetByIdAsync(id);
		audit.SendDateTime = DateTime.UtcNow;

		await _unitOfWork.OPCAudit.AddAsync(audit);
		await _unitOfWork.SaveChangesAsync();

		return audit;
	}

	public async Task<OPCAudit> UpdateOPCAuditInputMessageAsync(Guid id, string messageInput)
	{
		var audit = await _unitOfWork.OPCAudit.GetByIdAsync(id);

		audit.MessageInput = messageInput;
		audit.MessageInputDateTime = DateTime.UtcNow;

		await _unitOfWork.SaveChangesAsync();

		return audit;
	}

	public async Task<OPCAudit> UpdateOPCAuditOutputMessageAsync(Guid id, string messageOutput)
	{
		var audit = await _unitOfWork.OPCAudit.GetByIdAsync(id);

		audit.MessageOutput = messageOutput;
		audit.MessageOutputDateTime = DateTime.UtcNow;

		await _unitOfWork.SaveChangesAsync();

		return audit;
	}

	public async Task DeleteOPCAudit(Guid id)
	{
		await _unitOfWork.OPCAudit.Delete(id);
		await _unitOfWork.SaveChangesAsync();

	}
}
