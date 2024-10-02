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
using ProtonVPN.Client.Contracts.Services.Navigation;

namespace ProtonVPN.Client.UI.Main.Settings;

public partial class SettingsPageViewModel : PageViewModelBase<IMainViewNavigator, ISettingsViewNavigator>
{
    public SettingsPageViewModel(
        IMainViewNavigator parentViewNavigator,
        ISettingsViewNavigator childViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(parentViewNavigator, childViewNavigator, localizer, logger, issueReporter)
    {
    }

    [RelayCommand]
    private Task CloseAsync()
    {
        return ParentViewNavigator.NavigateToHomeViewAsync();
    }
}