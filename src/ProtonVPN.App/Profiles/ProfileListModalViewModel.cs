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
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Modals;
using ProtonVPN.Translations;

namespace ProtonVPN.Profiles
{
    internal class ProfileListModalViewModel : BaseModalViewModel, IProfileSyncStatusAware
    {
        private readonly ProfileManager _profileManager;
        private readonly IModals _modals;
        private readonly IDialogs _dialogs;
        private readonly ProfileViewModelFactory _profileHelper;
        private readonly IVpnManager _vpnManager;

        private ProfileSyncStatus _profileSyncStatus = ProfileSyncStatus.Succeeded;

        public ProfileListModalViewModel(
            ProfileManager profileManager,
            ProfileViewModelFactory profileHelper,
            IModals modals,
            IDialogs dialogs,
            IVpnManager vpnManager,
            ProfileSyncViewModel profileSync)
        {
            ProfileSync = profileSync;
            _profileHelper = profileHelper;
            _profileManager = profileManager;
            _modals = modals;
            _dialogs = dialogs;
            _vpnManager = vpnManager;

            ConnectCommand = new RelayCommand<ProfileViewModel>(ConnectAction);
            RemoveCommand = new RelayCommand<ProfileViewModel>(RemoveAction);
            EditCommand = new RelayCommand<ProfileViewModel>(EditProfileAction);
            CreateProfileCommand = new RelayCommand(CreateProfileAction);
            TroubleshootCommand = new RelayCommand(TroubleshootAction);
        }

        private IReadOnlyList<ProfileViewModel> _profiles;
        public IReadOnlyList<ProfileViewModel> Profiles
        {
            get => _profiles;
            set => Set(ref _profiles, value);
        }

        public ICommand RemoveCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand ConnectCommand { get; set; }
        public ICommand CreateProfileCommand { get; set; }
        public ICommand TroubleshootCommand { get; set; }

        public ProfileSyncViewModel ProfileSync { get; }

        protected override async void OnActivate()
        {
            await LoadProfiles();
        }

        public override async void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            base.OnAppSettingsChanged(e);

            if (e.PropertyName.Equals(nameof(IAppSettings.Profiles)))
            {
                await LoadProfiles();
            }
        }

        public void OnProfileSyncStatusChanged(ProfileSyncStatus status, string errorMessage, DateTime changesSyncedAt)
        {
            _profileSyncStatus = status;
            ApplyProfileSyncStatus(Profiles);
        }

        private void ApplyProfileSyncStatus(IEnumerable<ProfileViewModel> profiles)
        {
            if (profiles == null)
                return;

            foreach (var profile in profiles)
            {
                profile.OnProfileSyncStatusChanged(_profileSyncStatus);
            }
        }

        private async void RemoveAction(ProfileViewModel viewModel)
        {
            if (viewModel == null) return;
            var profile = await _profileManager.GetProfileById(viewModel.Id);
            var result = _dialogs.ShowQuestion(Translation.Get("Profiles_msg_DeleteConfirm"));

            if (result.HasValue && result.Value)
            {
                await _profileManager.RemoveProfile(profile);
            }
        }

        private async Task LoadProfiles()
        {
            var profiles = (await _profileHelper.GetProfiles())
                .OrderByDescending(p => p.IsPredefined)
                .ThenBy(p => p.Name)
                .ToList();

            ApplyProfileSyncStatus(profiles);

            Profiles = profiles;
        }

        private async void ConnectAction(ProfileViewModel viewModel)
        {
            if (viewModel == null) return;
            var profile = await _profileManager.GetProfileById(viewModel.Id);
            if (profile == null) return;

            await _vpnManager.ConnectAsync(profile);
        }

        private async void EditProfileAction(ProfileViewModel viewModel)
        {
            if (viewModel == null) return;
            var profile = await _profileManager.GetProfileById(viewModel.Id);
            if (profile == null) return;

            dynamic options = new ExpandoObject();
            options.Profile = profile;
            _modals.Show<ProfileFormModalViewModel>(options);
        }

        private void CreateProfileAction()
        {
            _modals.Show<ProfileFormModalViewModel>();
        }

        private void TroubleshootAction()
        {
            _modals.Show<TroubleshootModalViewModel>();
        }
    }
}
