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

using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Sidebar.ChangeServer;

namespace ProtonVPN.Servers
{
    public class UpsellBannerViewModel : ViewModel, IServerListItem, IHandle<ChangeServerTimeLeftMessage>
    {
        public UpsellBannerViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }

        public void OnVpnStateChanged(VpnState state)
        {
        }

        public string Id => string.Empty;
        public string Name => string.Empty;
        public bool Maintenance => false;
        public bool Connected => false;

        private bool _isToShowNotTheCountryYouWanted;
        public bool IsToShowNotTheCountryYouWanted
        {
            get => _isToShowNotTheCountryYouWanted;
            set => Set(ref _isToShowNotTheCountryYouWanted, value);
        }

        public async Task HandleAsync(ChangeServerTimeLeftMessage message, CancellationToken cancellationToken)
        {
            IsToShowNotTheCountryYouWanted = message.TimeLeftInSeconds > 0;
        }
    }
}