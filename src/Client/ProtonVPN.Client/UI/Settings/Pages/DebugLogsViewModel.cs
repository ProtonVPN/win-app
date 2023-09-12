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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Parameters;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Configuration;

namespace ProtonVPN.Client.UI.Settings.Pages;

public partial class DebugLogsViewModel : SettingsPageViewModelBase
{
    private readonly IConfiguration _configuration;
    private readonly IUrls _urls;

    public override string? Title => Localizer.Get("Settings_Support_DebugLogs");

    public DebugLogsViewModel(IMainViewNavigator viewNavigator, ILocalizationProvider localizationProvider, ISettings settings, IConfiguration configuration, IUrls urls)
        : base(viewNavigator, localizationProvider, settings)
    {
        _configuration = configuration;
        _urls = urls;
    }

    [RelayCommand]
    public async Task OpenApplicationLogsAsync()
    {
        await OpenLogsFolderAsync(_configuration.AppLogFolder);
    }

    [RelayCommand]
    public async Task OpenServiceLogsAsync()
    {
        await OpenLogsFolderAsync(_configuration.ServiceLogFolder);
    }

    private async Task OpenLogsFolderAsync(string logFolder)
    {
        if (!Directory.Exists(logFolder))
        {
            await ViewNavigator.ShowMessageAsync(new MessageDialogParameters
            {
                Title = Localizer.Get("Settings_Support_DebugLogs"),
                Message = $"{Localizer.Get("Settings_Support_DebugLogs_ErrorMessage")}\n({logFolder})",
                CloseButtonText = Localizer.Get("Common_Actions_Close")
            });
            return;
        }

        _urls.NavigateTo(logFolder);
    }
}