using System;
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    internal class ReconnectManager : IVpnStateAware
    {
        private VpnState _state;
        private readonly IApiClient _apiClient;
        private readonly ProfileManager _profileManager;
        private readonly IVpnManager _vpnManager;
        private readonly ISchedulerTimer _timer;
        private readonly ServerManager _serverManager;
        private readonly IServerUpdater _serverUpdater;

        public ReconnectManager(
            Common.Configuration.Config config,
            IApiClient apiClient,
            ProfileManager profileManager,
            ServerManager serverManager,
            IVpnManager vpnManager,
            IScheduler scheduler,
            IServerUpdater serverUpdater)
        {
            _serverUpdater = serverUpdater;
            _serverManager = serverManager;
            _vpnManager = vpnManager;
            _profileManager = profileManager;
            _apiClient = apiClient;

            _timer = scheduler.Timer();
            _timer.Interval = config.MaintenanceCheckInterval;
            _timer.Tick += OnTimerTick;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _state = e.State;

            if (e.State.Status == VpnStatus.Connected)
            {
                if (!_timer.IsEnabled)
                {
                    _timer.Start();
                }
            }
            else
            {
                if (_timer.IsEnabled)
                {
                    _timer.Stop();
                }
            }

            return Task.CompletedTask;
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            if (!await ServerOffline())
            {
                return;
            }

            await _serverUpdater.Update();
            var fastestProfile = await _profileManager.GetFastestProfile();
            await _vpnManager.Connect(fastestProfile);
        }

        private async Task<bool> ServerOffline()
        {
            var server = _serverManager.GetPhysicalServerByExitIp(_state.Server.ExitIp);
            if (server == null)
            {
                //Server removed from api
                return true;
            }

            try
            {
                var result = await _apiClient.GetServerAsync(server.Id);
                if (!result.Success)
                {
                    return false;
                }

                return result.Value.Server.Status == 0;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}
