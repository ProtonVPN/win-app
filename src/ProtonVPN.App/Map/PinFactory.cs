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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Name;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Map.ViewModels.Pins;
using ProtonVPN.Servers;
using ProtonVPN.Vpn.Connectors;

namespace ProtonVPN.Map
{
    internal class PinFactory :
        IVpnStateAware, 
        IVpnPlanAware, 
        ILogoutAware, 
        IServersAware
    {
        private readonly MapLineManager _mapLineManager;
        private readonly IUserStorage _userStorage;
        private Dictionary<string, AbstractPinViewModel> _pins = new Dictionary<string, AbstractPinViewModel>();
        private List<AbstractPinViewModel> _secureCorePins = new List<AbstractPinViewModel>();
        private VpnStateChangedEventArgs _vpnState;
        private readonly ServerManager _serverManager;
        private readonly ServerConnector _serverConnector;
        private readonly CountryConnector _countryConnector;

        public PinFactory(
            MapLineManager mapLineManager,
            IUserStorage userStorage,
            ServerManager serverManager,
            ServerConnector serverConnector,
            CountryConnector countryConnector)
        {
            _serverManager = serverManager;
            _serverConnector = serverConnector;
            _countryConnector = countryConnector;
            _mapLineManager = mapLineManager;
            _userStorage = userStorage;
        }

        public event EventHandler<EventArgs> PinsChanged;

        public void BuildPins()
        {
            BuildStandardPins();
            BuildSecureCorePins();
            SetLines();
        }

        public Dictionary<string, AbstractPinViewModel> GetPins()
        {
            return _pins;
        }

        public void OnUserLoggedOut()
        {
            _pins = new Dictionary<string, AbstractPinViewModel>();
            _secureCorePins = new List<AbstractPinViewModel>();
        }

        public List<AbstractPinViewModel> GetSecureCorePins()
        {
            return _secureCorePins;
        }

        public void HideTooltips()
        {
            var pins = GetPins();
            foreach (var pin in pins)
            {
                pin.Value.ShowTooltip = false;
            }
        }

        public void HideSecureCorePins()
        {
            var pins = GetSecureCorePins().OfType<SecureCorePinViewModel>();
            foreach (var pin in pins)
            {
                pin.ShowTooltip = false;
            }
        }

        public void HideExitPins()
        {
            var pins = GetSecureCorePins().OfType<ExitNodePinViewModel>();
            foreach (var pin in pins)
            {
                pin.ShowTooltip = false;
            }
        }

        public void SetSecureCorePinStates(PinStates state)
        {
            var pins = GetSecureCorePins();
            foreach (var pin in pins)
            {
                pin.State = state;
            }
        }

        public void SetPinStates(PinStates state)
        {
            var pins = GetPins();
            foreach (var pin in pins)
            {
                pin.Value.State = state;
            }
        }

        public void HideExitNodeTooltip(AbstractPinViewModel entryNodePin)
        {
            var pins = GetSecureCorePins();
            var list = pins.Where(c => !c.CountryCode.Equals(entryNodePin.CountryCode));
            foreach (var pin in list)
            {
                pin.ShowTooltip = false;
            }
        }

        public void ShowSecureCoreTooltipByExitNode(string countryCode)
        {
            var servers = _serverManager.GetServers(new SecureCoreServer());
            foreach (var server in servers)
            {
                if (server.ExitCountry.Equals(countryCode))
                {
                    GetSecureCorePinByCountry(server.EntryCountry).ShowTooltip = true;
                }
            }
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnState = e;

            var pins = GetPins();
            foreach (var pin in pins)
            {
                pin.Value.OnVpnStateChanged(e);
            }

            var secureCorePins = GetSecureCorePins();
            foreach (var pin in secureCorePins)
            {
                pin.OnVpnStateChanged(e);
            }

            return Task.CompletedTask;
        }

        public Task OnVpnPlanChangedAsync(VpnPlanChangedEventArgs e)
        {
            RebuildPins();

            return Task.CompletedTask;
        }

        public void OnServersUpdated()
        {
            RebuildPins();
        }

        private void RebuildPins()
        {
            BuildPins();
            PinsChanged?.Invoke(this, EventArgs.Empty);

            if (_vpnState != null)
                OnVpnStateChanged(_vpnState);
        }

        private void SetLines()
        {
            _mapLineManager.SetPins(_pins);
            _mapLineManager.SetSecureCorePins(_secureCorePins);
            _mapLineManager.BuildLines();
        }

        private void BuildStandardPins()
        {
            var user = _userStorage.GetUser();
            var countries = _serverManager.GetCountries();

            _pins = countries
                .Select(c => new { CountryCode = c, Pin = GetPin(c, user.MaxTier) })
                .Where(c => c.Pin != null)
                .OrderBy(p => _serverManager.CountryHasAvailableServers(p.CountryCode, user.MaxTier))
                .ThenBy(p => p.Pin.VerticalOffset)
                .ToDictionary(p => p.CountryCode, p => p.Pin);
        }

        private void BuildSecureCorePins()
        {
            var secureCorePins = new List<AbstractPinViewModel>();
            var servers = _serverManager.GetServers(new SecureCoreServer());

            foreach (string secureCoreCountryCode in SecureCoreCountry.CountryCodes)
            {
                var pin = GetSecureCorePin(secureCoreCountryCode);
                secureCorePins.Add(pin);
            }

            foreach (var server in servers)
            {
                var pin = GetExitNodePin(server.ExitCountry);
                if (secureCorePins.FirstOrDefault(c => c.CountryCode.Equals(server.ExitCountry)) == null)
                {
                    pin.Highlighted = _userStorage.GetUser().MaxTier >= ServerTiers.Plus;
                    secureCorePins.Add(pin);
                }
            }

            _secureCorePins = secureCorePins;
        }

        private SecureCorePinViewModel GetSecureCorePinByCountry(string countryCode)
        {
            return GetSecureCorePins().OfType<SecureCorePinViewModel>()
                .FirstOrDefault(c => c.CountryCode.Equals(countryCode));
        }

        private AbstractPinViewModel GetPin(string countryCode, sbyte userTier)
        {
            var location = new CountryLocation(countryCode);
            if (location.Coordinates().X.Equals(0) && location.Coordinates().Y.Equals(0))
                return null;

            var pin = new PinViewModel(countryCode, _countryConnector, this)
            {
                ConnectionName = new StandardServerName
                {
                    Name = Countries.GetName(countryCode)
                },
                Highlighted = _serverManager.CountryHasAvailableServers(countryCode, userTier)
            };

            return pin;
        }

        private SecureCorePinViewModel GetSecureCorePin(string countryCode)
        {
            return new SecureCorePinViewModel(
                countryCode,
                _mapLineManager,
                this);
        }

        private ExitNodePinViewModel GetExitNodePin(string countryCode)
        {
            return new ExitNodePinViewModel(
                countryCode,
                _serverManager,
                _mapLineManager,
                _serverConnector,
                this);
        }
    }
}
