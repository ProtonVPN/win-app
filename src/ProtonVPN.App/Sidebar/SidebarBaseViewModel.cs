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

using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Core.Settings;
using System.ComponentModel;
using System.Windows.Input;

namespace ProtonVPN.Sidebar
{
    public class SidebarBaseViewModel : Screen
    {
        private readonly IAppSettings _appSettings;
        private readonly SidebarManager _sidebarManager;
        private bool _sidebarMode;

        public SidebarBaseViewModel(IAppSettings appSettings, SidebarManager sidebarManager)
        {
            _appSettings = appSettings;
            _sidebarManager = sidebarManager;
            _appSettings.PropertyChanged += OnAppSettingsChanged;
            ToggleSidebarModeCommand = new RelayCommand(ToggleSidebarModeAction);
        }

        public ICommand ToggleSidebarModeCommand { get; }

        public virtual void Load()
        {
            SetSidebarMode();
        }

        public bool SidebarMode
        {
            get => _sidebarMode;
            set => Set(ref _sidebarMode, value);
        }

        private void ToggleSidebarModeAction()
        {
            SidebarMode = !SidebarMode;
            _sidebarManager.ToggleSidebar();
        }

        private void OnAppSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SidebarMode")
            {
                SetSidebarMode();
            }
        }

        private void SetSidebarMode()
        {
            _sidebarMode = _appSettings.SidebarMode;
            NotifyOfPropertyChange(() => SidebarMode);
        }
    }
}
