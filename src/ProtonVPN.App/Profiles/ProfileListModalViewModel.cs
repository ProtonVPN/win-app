﻿/*
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
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Threading;
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
    internal class ProfileListModalViewModel : BaseModalViewModel
    {
        private readonly ProfileManager _profileManager;
        private readonly IModals _modals;
        private readonly IDialogs _dialogs;
        private readonly ProfileViewModelFactory _profileHelper;
        private readonly IVpnManager _vpnManager;

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

        public ProfileListModalViewModel(
            ProfileManager profileManager,
            ProfileViewModelFactory profileHelper,
            IModals modals,
            IDialogs dialogs,
            IVpnManager vpnManager)
        {
            _profileHelper = profileHelper;
            _profileManager = profileManager;
            _modals = modals;
            _dialogs = dialogs;
            _vpnManager = vpnManager;

            ConnectCommand = new RelayCommand<ProfileViewModel>(ConnectAction);
            RemoveCommand = new RelayCommand<ProfileViewModel>(RemoveAction);
            EditCommand = new RelayCommand<ProfileViewModel>(EditProfileAction);
            CreateProfileCommand = new RelayCommand(CreateProfileAction);
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
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

        private async void RemoveAction(ProfileViewModel viewModel)
        {
            if (viewModel == null)
            {
                return;
            }

            Profile profile = await _profileManager.GetProfileById(viewModel.Id);
            bool? result = await _dialogs.ShowQuestionAsync(Translation.Get("Profiles_msg_DeleteConfirm"));

            if (result.HasValue && result.Value)
            {
                await _profileManager.RemoveProfile(profile);
            }
        }

        private async Task LoadProfiles()
        {
            List<ProfileViewModel> profiles = (await _profileHelper.GetProfiles())
                .OrderByDescending(p => p.IsPredefined)
                .ThenBy(p => p.Name)
                .ToList();

            Profiles = profiles;
        }

        private async void ConnectAction(ProfileViewModel viewModel)
        {
            if (viewModel != null)
            {
                Profile profile = await _profileManager.GetProfileById(viewModel.Id);
                if (profile != null)
                {
                    await _vpnManager.ConnectAsync(profile);
                }
            }
        }

        private async void EditProfileAction(ProfileViewModel viewModel)
        {
            if (viewModel != null)
            {
                Profile profile = await _profileManager.GetProfileById(viewModel.Id);
                if (profile != null)
                {
                    dynamic options = new ExpandoObject();
                    options.Profile = profile;
                    await _modals.ShowAsync<ProfileFormModalViewModel>(options);
                }
            }
        }

        private async void CreateProfileAction()
        {
            await _modals.ShowAsync<ProfileFormModalViewModel>();
        }
    }
}
