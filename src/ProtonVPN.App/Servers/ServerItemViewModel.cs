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

using System.Collections.Generic;
using ProtonVPN.Common.Vpn;
using ProtonVPN.ConnectionInfo;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Name;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Partners;

namespace ProtonVPN.Servers
{
    public class ServerItemViewModel : ViewModel, IServerListItem
    {
        private bool _connecting;
        private bool _connected;
        private bool _showIp = true;

        private readonly List<PartnerType> _partnerTypes;
        private readonly sbyte _userTier;
        private readonly InfoPopupViewModel _streamingInfoPopupViewModel;

        public ServerItemViewModel(Server server, List<PartnerType> partnerTypes, sbyte userTier,
            InfoPopupViewModel streamingInfoPopupViewModel = null)
        {
            _partnerTypes = partnerTypes;
            _userTier = userTier;
            _streamingInfoPopupViewModel = streamingInfoPopupViewModel;

            AssignServer(server);
            SetServerFeatures(server);
            ConnectionInfoViewModel = new ConnectionInfoViewModel(server);
        }

        public ConnectionInfoViewModel ConnectionInfoViewModel { get; }

        public bool Connecting
        {
            get => _connecting;
            set => Set(ref _connecting, value);
        }

        public bool Connected
        {
            get => _connected;
            set => Set(ref _connected, value);
        }

        public bool ShowIp
        {
            get => _showIp;
            set => Set(ref _showIp, value);
        }

        public Server Server { get; set; }

        public string Name => Server.Name;

        public IName ConnectionName { get; set; }

        private bool _upgradeRequired;

        public bool UpgradeRequired
        {
            get => _upgradeRequired;
            set => Set(ref _upgradeRequired, value);
        }

        public List<IServerFeature> Features { get; set; }

        public bool Maintenance => Server.Status == 0;

        public void SetServerFeatures(Server server)
        {
            List<IServerFeature> list = new();
            if (server.SupportsP2P())
            {
                list.Add(new P2PFeature());
            }

            if (server.SupportsTor())
            {
                list.Add(new TorFeature());
            }

            if (server.SupportsStreaming())
            {
                list.Add(new StreamingFeature(_streamingInfoPopupViewModel));
            }

            if (server.IsPartner())
            {
                foreach (PartnerType partnerType in _partnerTypes)
                {
                    foreach (Partner partner in partnerType.Partners)
                    {
                        if (partner.LogicalIDs.Contains(server.Id))
                        {
                            list.Add(new PartnerFeature(partner.Name, partner.IconUrl, _partnerTypes));
                        }
                    }
                }
            }

            Features = list;
        }

        public virtual void OnVpnStateChanged(VpnState state)
        {
            if (state.Server == null || !state.Server.Equals(Server))
            {
                SetDisconnected();
                return;
            }

            switch (state.Status)
            {
                case VpnStatus.Connected:
                    SetConnected();
                    break;
                case VpnStatus.Pinging:
                case VpnStatus.Connecting:
                case VpnStatus.Reconnecting:
                    SetConnecting();
                    break;
                default:
                    SetDisconnected();
                    break;
            }
        }

        private void AssignServer(Server server)
        {
            Server = server;

            ConnectionName = Server.GetServerName();
            SetIpVisibility(server);
            UpgradeRequired = _userTier < server.Tier;
            ConnectionName.UpgradeRequired = UpgradeRequired;
        }

        private void SetIpVisibility(Server server)
        {
            ShowIp = server.Tier > ServerTiers.Free;
        }

        protected void SetDisconnected()
        {
            Connecting = false;
            Connected = false;
        }

        protected void SetConnecting()
        {
            Connecting = true;
            Connected = false;
        }

        protected void SetConnected()
        {
            Connected = true;
            Connecting = false;
        }
    }
}