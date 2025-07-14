using Microsoft.AspNetCore.SignalR;

public interface IHubCommunicator
{
	Task SendMessage(string user, string message);
	Task NewTrayAtSeeder(Guid jobId, Guid trayId);
	Task RefreshDashboardSeeder();
	Task HeartBeat(int serverId);
	Task HeartBeatWorker();
	Task GeneralOPCNotification(string message);
	Task UpdateTrayState(Guid trayId);
    Task FireAlarmStateChanged(bool isAlarmActive);
}

namespace VHS.WebAPI.Hubs
{
    public class VHSNotificationHub : Hub<IHubCommunicator> 
	{
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendMessage(user, message);
        }

		public async Task NewTrayAtSeeder(Guid jobId, Guid trayId)
		{
			await Clients.All.NewTrayAtSeeder(jobId, trayId);
			await Clients.All.GeneralOPCNotification("NewTrayAtSeeder");
		}
		public async Task UpdateTrayState(Guid trayId)
		{
			await Clients.All.UpdateTrayState(trayId);
			await Clients.All.GeneralOPCNotification("UpdateTrayState");
		}
		public async Task HeartBeat(int serverId)
        {
            await Clients.All.HeartBeat(serverId);
        }

        public async Task HeartBeatWorker()
        {
            await Clients.All.HeartBeatWorker();
        }

		public async Task RefreshDashboardSeeder()
		{
			await Clients.All.HeartBeatWorker();
		}

		private async Task GeneralOPCNotification(string message)
		{
			await Clients.All.GeneralOPCNotification(message);
		}
	}
}
