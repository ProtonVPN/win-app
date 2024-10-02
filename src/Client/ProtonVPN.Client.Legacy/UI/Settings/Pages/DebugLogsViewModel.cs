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
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Legacy.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Legacy.Models.Activation;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.Models.Urls;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Legacy.UI.Settings.Pages;

public partial class DebugLogsViewModel : PageViewModelBase<IMainViewNavigator>
{
    private readonly IOverlayActivator _overlayActivator;
    private readonly IStaticConfiguration _staticConfig;
    private readonly IUrls _urls;

    public override string? Title => Localizer.Get("Settings_Support_DebugLogs");

    public DebugLogsViewModel(
        IMainViewNavigator viewNavigator, 
        ILocalizationProvider localizationProvider,
        IOverlayActivator overlayActivator,
        IStaticConfiguration staticConfig, 
        IUrls urls,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(viewNavigator, 
               localizationProvider,
               logger,
               issueReporter)
    {
        _overlayActivator = overlayActivator;
        _staticConfig = staticConfig;
        _urls = urls;
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
            await _overlayActivator.ShowMessageAsync(new MessageDialogParameters
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