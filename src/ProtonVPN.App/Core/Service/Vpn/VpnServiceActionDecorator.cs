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

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Common.PortForwarding;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnServiceActionDecorator : IVpnServiceManager
    {
        private readonly ISafeServiceAction _safeServiceAction;
        private readonly IVpnServiceManager _decorated;
        private readonly IModals _modals;
        private readonly IService _baseFilteringEngineService;

        public VpnServiceActionDecorator(
            ISafeServiceAction safeServiceAction,
            IVpnServiceManager decorated,
            IModals modals,
            IService baseFilteringEngineService)
        {
            _safeServiceAction = safeServiceAction;
            _decorated = decorated;
            _modals = modals;
            _baseFilteringEngineService = baseFilteringEngineService;
        }

        public async Task Connect(VpnConnectionRequest connectionRequest)
        {
            await InvokeAction(async() =>
            {
                await _decorated.Connect(connectionRequest);
                return Result.Ok();
            });
        }

        public async Task UpdateAuthCertificate(string certificate)
        {
            await InvokeAction(async() =>
            {
                await _decorated.UpdateAuthCertificate(certificate);
                return Result.Ok();
            });
        }

        public async Task Disconnect(VpnError error,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            await InvokeAction(async() =>
            {
                await _decorated.Disconnect(error);
                return Result.Ok();
            });
        }

        public async Task<InOutBytes> Total()
        {
            return await _decorated.Total();
        }

        public async Task RepeatState()
        {
            if (_baseFilteringEngineService.Running())
            {
                await _decorated.RepeatState();
            }
        }

        public void RegisterVpnStateCallback(Action<VpnStateChangedEventArgs> onVpnStateChanged)
        {
            _decorated.RegisterVpnStateCallback(onVpnStateChanged);
        }

        public void RegisterServiceSettingsStateCallback(Action<ServiceSettingsStateChangedEventArgs> onServiceSettingsStateChanged)
        {
            _decorated.RegisterServiceSettingsStateCallback(onServiceSettingsStateChanged);
        }

        public void RegisterPortForwardingStateCallback(Action<PortForwardingState> onPortForwardingStateChanged)
        {
            _decorated.RegisterPortForwardingStateCallback(onPortForwardingStateChanged);
        }

        public void RegisterConnectionDetailsChangeCallback(Action<ConnectionDetails> callback)
        {
            _decorated.RegisterConnectionDetailsChangeCallback(callback);
        }

        private async Task InvokeAction(Func<Task<Result>> action)
        {
            if (!_baseFilteringEngineService.Running())
            {
                _modals.Show<BfeWarningModalViewModel>();
                return;
            }

            await _safeServiceAction.InvokeServiceAction(action);
        }
    }
}