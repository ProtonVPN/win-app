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

using ProtonVPN.Common.Helpers;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Servers;

namespace ProtonVPN.ConnectionInfo
{
    public class ConnectionInfoViewModel : ViewModel
    {
        public object ServerInfo { get; set; }

        public string Title { get; }

        private string _load;
        public string Load
        {
            get => _load;
            set => Set(ref _load, value);
        }

        private int _loadNumber;
        public int LoadNumber
        {
            get => _loadNumber;
            set => Set(ref _loadNumber, value);
        }

        public object Ip { get; }
        public bool SecureCore { get; }
        public bool PremiumServer { get; }
        public bool P2PServer { get; }
        public bool TorServer { get; }

        private bool _showTooltip;
        public bool ShowTooltip
        {
            get => _showTooltip;
            set => Set(ref _showTooltip, value);
        }

        public ConnectionInfoViewModel(Server server)
        {
            Ensure.NotNull(server, nameof(server));

            LoadNumber = server.Load;
            Load = $"{server.Load}%";
            Ip = new Ip {Address = server.ExitIp};
            PremiumServer = server.Tier.Equals(ServerTiers.Plus);
            P2PServer = server.SupportsP2P();
            TorServer = server.SupportsTor();
            ServerInfo = new DefaultInfo
            {
                Country = Countries.GetName(server.EntryCountry),
                ServerName = server.Name
            };

            if (server.IsSecureCore())
            {
                SecureCore = true;

                ServerInfo = new SecureCoreInfo
                {
                    EntryCountry = Countries.GetName(server.EntryCountry),
                    ExitCountry = Countries.GetName(server.ExitCountry)
                };
            }

            if (server.IsSecureCore() || server.IsFree())
            {
                Ip = new AutoAssignedIp();
            }
        }
    }
}
