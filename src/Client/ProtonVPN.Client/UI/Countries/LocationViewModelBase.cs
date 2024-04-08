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
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Models.Activation.Custom;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Countries;

public abstract partial class LocationViewModelBase : ViewModelBase
{
    protected readonly IMainViewNavigator MainViewNavigator;
    protected readonly IConnectionManager ConnectionManager;
    private readonly ISettings _settings;
    private readonly IUpsellCarouselDialogActivator _upsellCarouselDialogActivator;

    protected abstract ModalSources UpsellModalSources { get; }

    protected LocationViewModelBase(
        ILocalizationProvider localizationProvider,
        IMainViewNavigator mainViewNavigator,
        IConnectionManager connectionManager,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IUpsellCarouselDialogActivator upsellCarouselDialogActivator)
        : base(localizationProvider, logger, issueReporter)
    {
        MainViewNavigator = mainViewNavigator;
        ConnectionManager = connectionManager;
        _settings = settings;
        _upsellCarouselDialogActivator = upsellCarouselDialogActivator;

        ConnectionDetails = ConnectionManager.CurrentConnectionDetails;
    }

    public ConnectionDetails? ConnectionDetails { get; }

    public abstract bool IsActiveConnection { get; }

    public bool IsFreeUser => !_settings.IsPaid;
    public bool IsUnderMaintenance { get; set; }
    public bool IsEnabled => !IsUnderMaintenance && !IsFreeUser;

    protected abstract ConnectionIntent ConnectionIntent { get; }

    [RelayCommand]
    public async Task ConnectAsync()
    {
        if (IsFreeUser)
        {
            _upsellCarouselDialogActivator.ShowDialog(UpsellModalSources);
            return;
        }

        await MainViewNavigator.NavigateToAsync<HomeViewModel>();
        await ConnectionManager.ConnectAsync(ConnectionIntent);
    }
}