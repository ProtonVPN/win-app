/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.Logging.Categorization.Events.UserInterfaceLogs;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals;

namespace ProtonVPN.Core
{
    public class AppExitHandler : IVpnStateAware, IServiceSettingsStateAware
    {
        private readonly ILogger _logger;
        private readonly IModals _modals;
        private readonly VpnService _vpnService;
        private VpnStatus _lastVpnStatus = VpnStatus.Disconnected;
        private bool _isNetworkBlocked;

        public bool PendingExit { get; private set; }

        public AppExitHandler(ILogger logger, IModals modals, VpnService vpnService)
        {
            _logger = logger;
            _vpnService = vpnService;
            _modals = modals;
        }

        public void RequestExit(
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _logger.Info<UserInterfaceExitLog>("App exit requested by the user.", sourceFilePath: sourceFilePath,
                sourceMemberName: sourceMemberName, sourceLineNumber: sourceLineNumber);
            if (ShowModal())
            {
                bool? result = _modals.Show<ExitModalViewModel>();
                if (result == false)
                {
                    _logger.Info<UserInterfaceExitLog>("App exit canceled by the user.");
                    return;
                }
            }

            _logger.Info<AppStopLog>("Requesting app to shut down.");
            PendingExit = true;
            _vpnService.UnRegisterCallback();
            Application.Current.Shutdown();
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _lastVpnStatus = e.State.Status;
            _isNetworkBlocked = e.NetworkBlocked;
            return Task.CompletedTask;
        }

        public void OnServiceSettingsStateChanged(ServiceSettingsStateChangedEventArgs e)
        {
            _isNetworkBlocked = e.IsNetworkBlocked;
        }

        private bool ShowModal()
        {
            return (_lastVpnStatus != VpnStatus.Disconnected &&
                    _lastVpnStatus != VpnStatus.Disconnecting) ||
                   _isNetworkBlocked;
        }
    }
}