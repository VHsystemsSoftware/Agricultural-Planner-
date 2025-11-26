using System.Net.Http.Json;
using VHS.OPC.Models;

namespace VHS.Client.Services
{
    public class SystemService
	{
        private readonly HttpClient _httpClient;

        public SystemService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> Version()
        {
            return await _httpClient.GetStringAsync($"api/system/version");
        }
		public async Task<List<OPCAlarm>> GetAlarms()
		{
			return await _httpClient.GetFromJsonAsync<List<OPCAlarm>>("api/alarms");
		}

	}
}
