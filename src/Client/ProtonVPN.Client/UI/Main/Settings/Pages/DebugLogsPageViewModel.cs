/*
 * Copyright (c) 2024 Proton AG
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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Client.UI.Main.Settings.Pages;

public partial class DebugLogsPageViewModel : SettingsPageViewModelBase
{
    private readonly IFilesBrowser _filesBrowser;
    private readonly IStaticConfiguration _staticConfig;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;

    public override string Title => Localizer.Get("Settings_Support_DebugLogs");

    public DebugLogsPageViewModel(
        IFilesBrowser filesBrowser,
        IStaticConfiguration staticConfig,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper)
        : base(requiredReconnectionSettings,
               mainViewNavigator,
               settingsViewNavigator,
               mainWindowOverlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager,
               viewModelHelper)
    {
        _filesBrowser = filesBrowser;
        _staticConfig = staticConfig;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
    }

    [RelayCommand]
    public async Task OpenApplicationLogsAsync()
    {
        await OpenLogsFolderAsync(_staticConfig.ClientLogsFolder);
    }

    [RelayCommand]
    public async Task OpenServiceLogsAsync()
    {
        await OpenLogsFolderAsync(_staticConfig.ServiceLogsFolder);
    }

    private async Task OpenLogsFolderAsync(string logFolder)
    {
        if (!Directory.Exists(logFolder))
        {
            await _mainWindowOverlayActivator.ShowMessageAsync(new MessageDialogParameters
            {
                Title = Localizer.Get("Settings_Support_DebugLogs"),
                Message = $"{Localizer.Get("Settings_Support_DebugLogs_ErrorMessage")}\n({logFolder})",
                CloseButtonText = Localizer.Get("Common_Actions_Close")
            });
            return;
        }

        _filesBrowser.OpenFolder(logFolder);
    }
}