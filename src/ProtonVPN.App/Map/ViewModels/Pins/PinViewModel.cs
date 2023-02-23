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

using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Vpn.Connectors;

namespace ProtonVPN.Map.ViewModels.Pins
{
    internal class PinViewModel : AbstractPinViewModel
    {
        private readonly CountryConnector _countryConnector;
        private readonly PinFactory _pinFactory;

        public bool UpgradeRequired { get;set; }

        public PinViewModel(
            string countryCode,
            CountryConnector countryConnector,
            PinFactory pinFactory) : base(countryCode)
        {
            _countryConnector = countryConnector;
            _pinFactory = pinFactory;
        }

        public override void OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            Connected = e.State.Server is Server server
                        && e.State.Status == VpnStatus.Connected
                        && server.EntryCountry == CountryCode;
        }

        protected override bool BeforeShowTooltip()
        {
            _pinFactory.HideTooltips();
            return true;
        }

        protected override bool BeforeHideTooltip()
        {
            return true;
        }

        protected override void ConnectAction()
        {
            Connect();
        }

        private async void Connect()
        {
            if (Connected)
            {
                await _countryConnector.Disconnect();
                return;
            }

            await _countryConnector.Connect(CountryCode);
        }
    }
}
