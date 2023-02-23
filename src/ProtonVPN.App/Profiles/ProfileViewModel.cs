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

using ProtonVPN.Common.Networking;
using ProtonVPN.ConnectionInfo;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Name;

namespace ProtonVPN.Profiles
{
    public class ProfileViewModel : ViewModel
    {
        private readonly ProfileSyncStatus _originSyncStatus;

        public ProfileViewModel(Profile profile)
        {
            Id = profile.Id;
            IsPredefined = profile.IsPredefined;
            Name = profile.Name;
            VpnProtocol = profile.VpnProtocol;
            Color = profile.ColorCode;
            SecureCore = profile.Features.IsSecureCore();
            Type = profile.ProfileType;

            if (profile.Server != null)
            {
                ConnectionInfoViewModel = new ConnectionInfoViewModel(profile.Server);
            }

            _syncStatus = profile.SyncStatus;
            _originSyncStatus = profile.SyncStatus;
        }

        public bool ShowBottomBorder { get; set; } = true;
        public bool UpgradeRequired { get; set; }

        public ConnectionInfoViewModel ConnectionInfoViewModel { get; }

        public string Id { get; }
        public bool IsPredefined { get; }
        public string Name { get; set; }
        public VpnProtocol VpnProtocol { get; }
        public string Color { get; }
        public bool SecureCore { get; set; }
        public bool Connected { get; set; }
        public Server Server { get; set; }
        public IName ConnectionName { get; set; }
        public ProfileType Type { get; set; }

        public string ConnectAutomationId => "Connect-" + Name;
        public string EditAutomationId => "Edit-" + Name;
        public string DeleteAutomationId => "Delete-" + Name;

        private ProfileSyncStatus _syncStatus;
        public ProfileSyncStatus SyncStatus
        {
            get => _syncStatus;
            private set => Set(ref _syncStatus, value);
        }

        public void OnProfileSyncStatusChanged(ProfileSyncStatus status)
        {
            SyncStatus = status == ProfileSyncStatus.Failed && _originSyncStatus == ProfileSyncStatus.InProgress
                ? ProfileSyncStatus.Failed
                : _originSyncStatus;
        }
    }
}