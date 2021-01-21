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

using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Settings
{
    internal class VpnReconnector : IVpnStateAware, IVpnReconnector
    {
        private readonly IVpnManager _vpnManager;
        private readonly IAppSettings _appSettings;

        private VpnStatus _vpnStatus;
        private Server _server;
        private bool _pendingReconnect;

        public VpnReconnector(IVpnManager vpnManager, IAppSettings appSettings)
        {
            _vpnManager = vpnManager;
            _appSettings = appSettings;
        }

        public async Task ReconnectAsync()
        {
            if (_vpnStatus == VpnStatus.Disconnected)
            {
                return;
            }

            _pendingReconnect = true;

            if (_appSettings.SecureCore)
            {
                await ReconnectWithSecureCoreAsync();
            }
            else
            {
                await ReconnectWithoutSecureCoreAsync();
            }
        }

        private async Task ReconnectWithSecureCoreAsync()
        {
            if (SecureCoreCountry.CountryCodes.Contains(_server.ExitCountry))
            {
                await _vpnManager.QuickConnect();
            }
            else
            {
                await ConnectToSecureCoreInSameCountryAsync();
            }
        }

        private async Task ConnectToSecureCoreInSameCountryAsync()
        {
            Profile profile = new Profile
            {
                Features = Features.SecureCore,
                ProfileType = ProfileType.Fastest,
                CountryCode = _server.ExitCountry
            };
            await _vpnManager.Connect(profile);
        }

        private async Task ReconnectWithoutSecureCoreAsync()
        {
            if (_appSettings.PortForwardingEnabled)
            {
                await ConnectToP2PInSameCountryIfPossibleAsync();
            }
            else
            {
                await ConnectToSameCountryAndFeaturesIfPossibleAsync();
            }
        }

        private async Task ConnectToP2PInSameCountryIfPossibleAsync()
        {
            Profile profile = new Profile
            {
                Features = Features.P2P,
                ProfileType = ProfileType.Fastest,
                CountryCode = _server.ExitCountry
            };
            Profile fallbackProfile = new Profile
            {
                Features = Features.P2P,
                ProfileType = ProfileType.Fastest
            };
            await _vpnManager.Connect(profile, fallbackProfile);
        }

        private async Task ConnectToSameCountryAndFeaturesIfPossibleAsync()
        {
            Profile profile = new Profile
            {
                Features = GetServerFeaturesExceptSecureCore(),
                ProfileType = ProfileType.Fastest,
                CountryCode = _server.ExitCountry
            };
            Profile fallbackProfile = new Profile
            {
                Features = Features.None,
                ProfileType = ProfileType.Fastest,
                CountryCode = _server.ExitCountry
            };
            await _vpnManager.Connect(profile, fallbackProfile);
        }

        private Features GetServerFeaturesExceptSecureCore()
        {
            return (Features)(_server.Features & ~(sbyte)Features.SecureCore);
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;
            _server = e.State.Server;

            if (_vpnStatus == VpnStatus.Connected || _vpnStatus == VpnStatus.Disconnected)
            {
                _pendingReconnect = false;
            }

            return Task.CompletedTask;
        }

        public bool IsPendingReconnect()
        {
            return _pendingReconnect;
        }
    }
}