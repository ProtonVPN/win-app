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

using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Updates.Contracts;

namespace ProtonVPN.Client.UI.Main.Home;

public class HomePageViewModel : PageViewModelBase<IMainViewNavigator>,
    IEventMessageReceiver<ClientUpdateStateChangedMessage>
{
    private readonly IUpdatesManager _updatesManager;

    public bool IsUpdateAvailable => _updatesManager.IsUpdateAvailable;

    public HomePageViewModel(
        IUpdatesManager updatesManager,
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(viewNavigator, localizer, logger, issueReporter)
    {
        _updatesManager = updatesManager;
    }

    public void Receive(ClientUpdateStateChangedMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(IsUpdateAvailable)));
    }
}