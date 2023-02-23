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

using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.FlashNotifications;
using ProtonVPN.Onboarding;

namespace ProtonVPN.Sidebar
{
    internal class SidebarViewModel : Screen,
        IOnboardingStepAware,
        ILoggedInAware
    {
        public CountriesViewModel Countries { get; }
        public SidebarProfilesViewModel Profiles { get; }
        public ConnectionStatusViewModel ConnectionStatus { get; }
        public FlashNotificationViewModel FlashNotification { get; }

        private readonly IAppSettings _appSettings;

        private const int CountriesTab = 0;
        private const int ProfilesTab = 1;

        private bool _showSecondOnboardingStep;
        private bool _showThirdOnboardingStep;
        private bool _isCountriesTabEnabled;

        public SidebarViewModel(
            IAppSettings appSettings,
            SidebarProfilesViewModel sidebarProfilesViewModel,
            ConnectionStatusViewModel connectionStatusViewModel,
            CountriesViewModel countriesViewModel,
            FlashNotificationViewModel flashNotificationsViewModel)
        {
            _appSettings = appSettings;
            CountriesTabCommand = new RelayCommand(OpenCountriesTabAction);
            ProfilesTabCommand = new RelayCommand(OpenProfilesTabAction);

            Countries = countriesViewModel;
            Profiles = sidebarProfilesViewModel;
            ConnectionStatus = connectionStatusViewModel;
            FlashNotification = flashNotificationsViewModel;
            
            IsCountriesTabEnabled = true;
        }

        public ICommand CountriesTabCommand { get; }
        public ICommand ProfilesTabCommand { get; }

        public bool IsCountriesTabEnabled
        {
            get => _isCountriesTabEnabled;
            set
            { 
                Set(ref _isCountriesTabEnabled, value);
                NotifyOfPropertyChange(() => IsProfilesTabEnabled);
            }
        }

        public bool IsProfilesTabEnabled => !_isCountriesTabEnabled;

        public bool ShowSecondOnboardingStep
        {
            get => _showSecondOnboardingStep;
            set => Set(ref _showSecondOnboardingStep, value);
        }

        public bool ShowThirdOnboardingStep
        {
            get => _showThirdOnboardingStep;
            set => Set(ref _showThirdOnboardingStep, value);
        }

        public void OnStepChanged(int step)
        {
            ShowSecondOnboardingStep = step == 2;
            ShowThirdOnboardingStep = step == 3;
            if (step == 4)
            {
                OpenCountriesTabAction();
            }
        }

        private void OpenProfilesTabAction()
        {
            IsCountriesTabEnabled = false;
            _appSettings.SidebarTab = ProfilesTab;
        }

        private void OpenCountriesTabAction()
        {
            IsCountriesTabEnabled = true;
            _appSettings.SidebarTab = CountriesTab;
        }

        public void OnUserLoggedIn()
        {
            if (_appSettings.SidebarTab == ProfilesTab)
            {
                OpenProfilesTabAction();
            }
        }
    }
}