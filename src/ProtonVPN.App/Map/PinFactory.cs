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
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
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
        private readonly IAppSettings _appSettings;
        private readonly MapLineManager _mapLineManager;
        private readonly IUserStorage _userStorage;
        private Dictionary<string, AbstractPinViewModel> _pins = new();
        private List<AbstractPinViewModel> _secureCorePins = new();
        private VpnStateChangedEventArgs _vpnState;
        private readonly ServerManager _serverManager;
        private readonly ServerConnector _serverConnector;
        private readonly CountryConnector _countryConnector;

        public PinFactory(
            IAppSettings appSettings,
            MapLineManager mapLineManager,
            IUserStorage userStorage,
            ServerManager serverManager,
            ServerConnector serverConnector,
            CountryConnector countryConnector)
        {
            _appSettings = appSettings;
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
            Dictionary<string, AbstractPinViewModel> pins = GetPins();
            foreach (KeyValuePair<string, AbstractPinViewModel> pin in pins)
            {
                pin.Value.ShowTooltip = false;
            }
        }

        public void HideSecureCorePins()
        {
            IEnumerable<SecureCorePinViewModel> pins = GetSecureCorePins().OfType<SecureCorePinViewModel>();
            foreach (SecureCorePinViewModel pin in pins)
            {
                pin.ShowTooltip = false;
            }
        }

        public void HideExitPins()
        {
            IEnumerable<ExitNodePinViewModel> pins = GetSecureCorePins().OfType<ExitNodePinViewModel>();
            foreach (ExitNodePinViewModel pin in pins)
            {
                pin.ShowTooltip = false;
            }
        }

        public void SetSecureCorePinStates(PinStates state)
        {
            List<AbstractPinViewModel> pins = GetSecureCorePins();
            foreach (AbstractPinViewModel pin in pins)
            {
                pin.State = state;
            }
        }

        public void SetPinStates(PinStates state)
        {
            Dictionary<string, AbstractPinViewModel> pins = GetPins();
            foreach (KeyValuePair<string, AbstractPinViewModel> pin in pins)
            {
                pin.Value.State = state;
            }
        }

        public void HideExitNodeTooltip(AbstractPinViewModel entryNodePin)
        {
            List<AbstractPinViewModel> pins = GetSecureCorePins();
            IEnumerable<AbstractPinViewModel> list = pins.Where(c => !c.CountryCode.Equals(entryNodePin.CountryCode));
            foreach (AbstractPinViewModel pin in list)
            {
                pin.ShowTooltip = false;
            }
        }

        public void ShowSecureCoreTooltipByExitNode(string countryCode)
        {
            IReadOnlyCollection<Server> servers = _serverManager.GetServers(new SecureCoreServer());
            foreach (Server server in servers)
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

            Dictionary<string, AbstractPinViewModel> pins = GetPins();
            foreach (KeyValuePair<string, AbstractPinViewModel> pin in pins)
            {
                pin.Value.OnVpnStateChanged(e);
            }

            List<AbstractPinViewModel> secureCorePins = GetSecureCorePins();
            foreach (AbstractPinViewModel pin in secureCorePins)
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
            {
                OnVpnStateChanged(_vpnState);
            }
        }

        private void SetLines()
        {
            _mapLineManager.SetPins(_pins);
            _mapLineManager.SetSecureCorePins(_secureCorePins);
            _mapLineManager.BuildLines();
        }

        private void BuildStandardPins()
        {
            User user = _userStorage.GetUser();
            IReadOnlyCollection<string> countries = _serverManager.GetCountries();

            _pins = countries
                .Select(c => new { CountryCode = c, Pin = GetPin(c, user.MaxTier) })
                .Where(c => c.Pin != null)
                .OrderBy(p => _serverManager.CountryHasAvailableServers(p.CountryCode, user.MaxTier))
                .ThenBy(p => p.Pin.VerticalOffset)
                .ToDictionary(p => p.CountryCode, p => p.Pin);
        }

        private void BuildSecureCorePins()
        {
            List<AbstractPinViewModel> secureCorePins = new();
            IReadOnlyCollection<Server> servers = _serverManager.GetServers(new SecureCoreServer());

            foreach (string secureCoreCountryCode in SecureCoreCountry.CountryCodes)
            {
                SecureCorePinViewModel pin = GetSecureCorePin(secureCoreCountryCode);
                secureCorePins.Add(pin);
            }

            bool isHighlighted = _userStorage.GetUser().Paid() || !_appSettings.FeatureFreeRescopeEnabled;
            foreach (Server server in servers)
            {
                ExitNodePinViewModel pin = GetExitNodePin(server.ExitCountry);
                if (secureCorePins.FirstOrDefault(c => c.CountryCode.Equals(server.ExitCountry)) == null)
                {
                    pin.Highlighted = isHighlighted;
                    pin.IsHighlightedOnDisconnect = isHighlighted;
                    pin.UpgradeRequired = !_userStorage.GetUser().Paid();
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
            CountryLocation location = new(countryCode);
            if (location.Coordinates().X.Equals(0) && location.Coordinates().Y.Equals(0))
            {
                return null;
            }

            bool countryHasAvailableServers = _serverManager.CountryHasAvailableServers(countryCode, userTier);
            bool isHighlighted = countryHasAvailableServers && (!_appSettings.FeatureFreeRescopeEnabled || _userStorage.GetUser().Paid());
            PinViewModel pin = new(countryCode, _countryConnector, this)
            {
                ConnectionName = new StandardServerName
                {
                    Name = Countries.GetName(countryCode)
                },
                Highlighted = isHighlighted,
                IsHighlightedOnDisconnect = isHighlighted,
                UpgradeRequired = _appSettings.FeatureFreeRescopeEnabled && !_userStorage.GetUser().Paid(),
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