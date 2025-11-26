using System.Net.Http.Json;
using VHS.Services.Notes.DTO;

namespace VHS.Client.Services.Notes;

public class NoteClientService
{
    private readonly HttpClient _httpClient;

    public NoteClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<NoteDTO>?> GetNotesForEntityAsync(Guid entityId)
    {
        return await _httpClient.GetFromJsonAsync<List<NoteDTO>>($"api/note/entity/{entityId}");
    }

    public async Task<NoteDTO?> GetNoteByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<NoteDTO>($"api/note/{id}");
    }

    public async Task<NoteDTO?> CreateNoteAsync(NoteDTO note)
    {
        var response = await _httpClient.PostAsJsonAsync("api/note", note);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<NoteDTO>();
    }

    public async Task UpdateNoteAsync(NoteDTO note)
    {
        await _httpClient.PutAsJsonAsync($"api/note/{note.Id}", note);
    }
}
