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
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Map.ViewModels.Pins
{
    internal class SecureCorePinViewModel : AbstractPinViewModel
    {
        private readonly MapLineManager _pinLineManager;
        private readonly PinFactory _pinFactory;
        private readonly VpnManager _vpnManager;

        public double PinWidth => 15;
        public double PinHeight => 15;

        public SecureCorePinViewModel(
            string countryCode,
            VpnManager vpnManager,
            MapLineManager pinLineManager,
            PinFactory pinFactory) : base(countryCode)
        {
            _vpnManager = vpnManager;
            _pinLineManager = pinLineManager;
            _pinFactory = pinFactory;
        }

        public override void OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            Connected = e.State.Server is Server server
                        && e.State.Status == VpnStatus.Connected
                        && server.EntryCountry.EqualsIgnoringCase(CountryCode);
        }

        protected override bool BeforeShowTooltip()
        {
            return true;
        }

        protected override bool BeforeHideTooltip()
        {
            return true;
        }

        protected override void ShowTooltipAction()
        {
            ShowTooltip = !ShowTooltip;
            if (ShowTooltip)
            {
                _pinFactory.HideSecureCorePins();
                _pinFactory.HideExitPins();
                ShowTooltip = true;
                _pinLineManager.HideExitLines();
                _pinLineManager.HideHomeLines();
                _pinLineManager.SetSecureCoreLinesVisibility(false);
                _pinLineManager.SetExitLinesVisibility(CountryCode, true);
                _pinLineManager.SetSecureCoreHomeLineVisibility(CountryCode, true);
            }
            else
            {
                if (!Connected)
                {
                    if (_vpnManager.Status.Equals(VpnStatus.Disconnected))
                    {
                        _pinLineManager.SetSecureCoreLinesVisibility(true);
                    }
                    _pinLineManager.SetSecureCoreHomeLineVisibility(CountryCode, false);
                }

                _pinFactory.HideExitNodeTooltip(this);
                _pinLineManager.SetExitLinesVisibility(CountryCode, false);
            }
        }

        protected override void ConnectAction()
        {
            throw new NotImplementedException();
        }
    }
}
