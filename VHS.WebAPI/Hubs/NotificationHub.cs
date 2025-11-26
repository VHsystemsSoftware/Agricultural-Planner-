using Microsoft.AspNetCore.SignalR;

public interface IHubCommunicator
{
    Task RefreshDashboardSeeder();
    Task RefreshDashboardHarvester();
    Task RefreshHome();
    Task HeartBeat(int serverId);
    Task HeartBeatWorker();
    Task GeneralOPCNotification(string message);
    Task UpdateTrayState(Guid trayId);
    Task FireAlarmStateChanged(bool isAlarmActive);
    Task AlarmStatusChanged(string alarmName);
    Task RefreshDashboardTransplanter();
}

namespace VHS.WebAPI.Hubs
{
    public class VHSNotificationHub : Hub<IHubCommunicator>
    {
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
            await Clients.All.RefreshDashboardSeeder();
        }
        public async Task RefreshDashboardHarvester()
        {
            await Clients.All.RefreshDashboardHarvester();
        }
		public async Task RefreshDashboardTransplanter()
		{
			await Clients.All.RefreshDashboardTransplanter();
		}
		public async Task RefreshHome()
        {
            await Clients.All.RefreshHome();
        }
        private async Task GeneralOPCNotification(string message)
        {
            await Clients.All.GeneralOPCNotification(message);
		}
		public async Task AlarmStatusChanged(string alarmName)
		{
			await Clients.All.AlarmStatusChanged(alarmName);
		}
	}
}
