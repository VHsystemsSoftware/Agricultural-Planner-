using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VHS.Services.Notes.DTO;
using VHS.Services.Notes;

namespace VHS.WebAPI.Controllers.Notes;

[ApiController]
[Route("api/[controller]")]
public class NoteController : ControllerBase
{
    private readonly INoteService _noteService;

    public NoteController(INoteService noteService)
    {
        _noteService = noteService;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    [HttpGet("entity/{entityId}")]
    [Authorize(Policy = "FarmManagerAndAbove")]
    public async Task<IActionResult> GetNotesForEntity(Guid entityId)
    {
        var notes = await _noteService.GetNotesForEntityAsync(entityId);
        return Ok(notes);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "FarmManagerAndAbove")]
    public async Task<IActionResult> GetNoteById(Guid id)
    {
        var note = await _noteService.GetNoteByIdAsync(id);
        if (note == null)
        {
            return NotFound();
        }
        return Ok(note);
    }

    [HttpPost]
    [Authorize(Policy = "FarmManagerAndAbove")]
    public async Task<IActionResult> CreateNote([FromBody] NoteDTO noteDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var createdNote = await _noteService.CreateNoteAsync(noteDto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetNoteById), new { id = createdNote.Id }, createdNote);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "FarmManagerAndAbove")]
    public async Task<IActionResult> UpdateNote(Guid id, [FromBody] NoteDTO noteDto)
    {
        if (id != noteDto.Id)
        {
            return BadRequest("ID mismatch");
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        await _noteService.UpdateNoteAsync(noteDto, GetCurrentUserId());
        return NoContent();
    }

}
