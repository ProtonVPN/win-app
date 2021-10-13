/*
 * Copyright (c) 2020 Proton Technologies AG
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

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnManager : IVpnManager
    {
        private readonly ITaskQueue _taskQueue = new SerialTaskQueue();
        private readonly IVpnServiceManager _vpnServiceManager;
        private readonly IVpnReconnector _vpnReconnector;
        private readonly IVpnConnector _vpnConnector;
        private readonly ILogger _logger;

        public VpnManager(
            IVpnServiceManager vpnServiceManager,
            IVpnReconnector vpnReconnector,
            IVpnConnector vpnConnector,
            ILogger logger)
        {
            _vpnServiceManager = vpnServiceManager;
            _vpnReconnector = vpnReconnector;
            _vpnConnector = vpnConnector;
            _logger = logger;
            _vpnConnector.VpnStateChanged += OnConnectorVpnStateChanged;
        }

        public event EventHandler<VpnStateChangedEventArgs> VpnStateChanged;

        private void OnConnectorVpnStateChanged(object sender, VpnStateChangedEventArgs e)
        {
            VpnStateChanged?.Invoke(this, e);
        }

        public void OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnConnector.OnVpnStateChanged(e);
        }

        public async Task ConnectAsync(Profile profile, Profile fallbackProfile = null,
            [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            await ExecuteAsync(memberName, sourceFilePath, sourceLineNumber, ConnectFunction(profile, fallbackProfile));
        }

        private Func<Task> ConnectFunction(Profile profile, Profile fallbackProfile)
        {
            return async () => await Enqueue(() => _vpnConnector.ConnectToBestProfileAsync(profile, fallbackProfile));
        }

        private async Task ExecuteAsync(string callerMemberName, string callerSourceFilePath, int callerSourceLineNumber, 
            Func<Task> func, [CallerMemberName] string memberName = "")
        {
            string callerClass;
            try
            {
                callerClass = Path.GetFileNameWithoutExtension(callerSourceFilePath);
            }
            catch (Exception e)
            {
                _logger.Error($"An exception occurred when getting the file name from the path '{callerSourceFilePath}'.", e);
                callerClass = callerSourceFilePath;
            }
            _logger.Info($"The '{memberName}' was called by {callerClass}.{callerMemberName}:{callerSourceLineNumber}.");
            await func();
        }

        public async Task QuickConnectAsync(
            [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            await ExecuteAsync(memberName, sourceFilePath, sourceLineNumber, QuickConnectFunction());
        }

        private Func<Task> QuickConnectFunction()
        {
            return async () => await Enqueue(() => _vpnConnector.QuickConnectAsync());
        }

        public async Task ReconnectAsync(VpnReconnectionSettings settings = null,
            [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            await ExecuteAsync(memberName, sourceFilePath, sourceLineNumber, ReconnectFunction(settings));
        }

        private Func<Task> ReconnectFunction(VpnReconnectionSettings settings = null)
        {
            return async () => await Enqueue(() => _vpnReconnector.ReconnectAsync(
                _vpnConnector.LastServer, _vpnConnector.LastProfile, settings));
        }

        public async Task DisconnectAsync(VpnError vpnError = VpnError.None,
            [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            await ExecuteAsync(memberName, sourceFilePath, sourceLineNumber, DisconnectFunction(vpnError));
        }

        private Func<Task> DisconnectFunction(VpnError vpnError = VpnError.None)
        {
            return async () => 
            {
                _vpnReconnector.OnDisconnectionRequest();
                await Enqueue(() => _vpnServiceManager.Disconnect(vpnError));
            };
        }

        public async Task GetStateAsync()
        {
            await _vpnServiceManager.RepeatState();
        }

        private Task Enqueue(Func<Task> function)
        {
            return _taskQueue.Enqueue(function);
        }
    }
}
