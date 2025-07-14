using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Data.Audit.Infrastructure;

namespace VHS.WebAPI.Controllers.OPCCommunication;

[ApiController]
[Route("api/opcaudit")]
[AllowAnonymous] // Temporary allowed, consider securing this endpoint
public class OPCAuditController : ControllerBase
{
	private readonly IUnitOfWorkAudit _unitOfWork;

	public OPCAuditController(IUnitOfWorkAudit unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	[HttpGet("range/{fromUnixDT}/{toUnixDT}")]
	public async Task<IActionResult> List(long fromUnixDT, long toUnixDT)
	{
		//var fromDateTime = DateTimeOffset.FromUnixTimeMilliseconds(fromUnixDT).UtcDateTime;
		//var toDateTime = DateTimeOffset.FromUnixTimeMilliseconds(toUnixDT).UtcDateTime;
		var data = (await _unitOfWork.OPCAudit
			.GetAllAsync()).OrderByDescending(x => x.ReceiveDateTime).Take(50);
		return Ok(data);
	}

}