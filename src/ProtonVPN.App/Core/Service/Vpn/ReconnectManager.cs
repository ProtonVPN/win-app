using System;
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Settings;
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
        private readonly IAppSettings _appSettings;

        public ReconnectManager(
            IAppSettings appSettings,
            IApiClient apiClient,
            ProfileManager profileManager,
            ServerManager serverManager,
            IVpnManager vpnManager,
            IScheduler scheduler,
            IServerUpdater serverUpdater)
        {
            _appSettings = appSettings;
            _serverUpdater = serverUpdater;
            _serverManager = serverManager;
            _vpnManager = vpnManager;
            _profileManager = profileManager;
            _apiClient = apiClient;

            _timer = scheduler.Timer();
            _timer.Tick += OnTimerTick;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (!_appSettings.FeatureMaintenanceTrackerEnabled)
            {
                return Task.CompletedTask;
            }

            _state = e.State;

            if (e.State.Status == VpnStatus.Connected)
            {
                if (!_timer.IsEnabled)
                {
                    _timer.Interval = _appSettings.MaintenanceCheckInterval;
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

            _serverManager.MarkServerUnderMaintenance(_state.Server.ExitIp);
            await _serverUpdater.Update();
            if (_appSettings.IsSmartReconnectEnabled())
            {
                await _vpnManager.ReconnectAsync(new VpnReconnectionSettings { IsToReconnectIfDisconnected = true, IsToExcludeLastServer = true });
            }
        }

        private async Task<bool> ServerOffline()
        {
            PhysicalServer server = _serverManager.GetPhysicalServerByServer(_state.Server);
            if (server == null)
            {
                //Server removed from api
                return true;
            }

            try
            {
                ApiResponseResult<PhysicalServerResponse> result = await _apiClient.GetServerAsync(server.Id);
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
