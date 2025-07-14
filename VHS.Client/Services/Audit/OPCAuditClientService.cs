using System.Net.Http.Json;
using VHS.Data.Models.Audit;

namespace VHS.Client.Services.Audit
{
    public class OPCAuditClientService
	{
        private readonly HttpClient _httpClient;

        public OPCAuditClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<OPCAudit>?> GetRangeAsync(long fromUnixDT, long toUndixDT)
        {
            return await _httpClient.GetFromJsonAsync<List<OPCAudit>>($"api/opcaudit/range/{fromUnixDT}/{toUndixDT}");
        }

       
    }
}
