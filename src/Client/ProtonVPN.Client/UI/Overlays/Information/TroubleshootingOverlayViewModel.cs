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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Services.Browsing;

namespace ProtonVPN.Client.UI.Overlays.Information;

public partial class TroubleshootingOverlayViewModel : OverlayViewModelBase<IMainWindowOverlayActivator>
{
    private readonly IUrls _urls;

    public TroubleshootingOverlayViewModel(
        IMainWindowOverlayActivator overlayActivator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IUrls urls)
        : base(overlayActivator, localizer, logger, issueReporter)
    {
        _urls = urls;
    }

    [RelayCommand]
    public void OpenStatusPage()
    {
        _urls.NavigateTo(_urls.ProtonStatusPage);
    }

    [RelayCommand]
    public void ContactUs()
    {
        _urls.NavigateTo(_urls.SupportForm);
    }
}