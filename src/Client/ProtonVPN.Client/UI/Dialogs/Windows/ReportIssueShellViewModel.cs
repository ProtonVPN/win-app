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

using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Dialogs.Overlays;
using ProtonVPN.Client.UI.Home;

namespace ProtonVPN.Client.UI.Dialogs.Windows;

public partial class ReportIssueShellViewModel : ShellViewModelBase
{
    public ReportIssueShellViewModel(IReportIssueViewNavigator viewNavigator, ILocalizationProvider localizationProvider) 
        : base(viewNavigator, localizationProvider)
    {
    }

    public override string Title => Localizer.Get("Dialogs_ReportIssue_Title");

    public void Initialize()
    {
        ViewNavigator.NavigateTo<HomeViewModel>();

        ViewNavigator.ShowOverlayAsync<ProtocolOverlayViewModel>();
    }
}