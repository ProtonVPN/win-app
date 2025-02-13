﻿/*
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
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Models.Connections;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;

public abstract partial class CountriesComponentViewModelBase : ActivatableViewModelBase, ICountriesComponent,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>
{
    protected readonly ISettings Settings;
    protected readonly IServersLoader ServersLoader;
    protected readonly ILocationItemFactory LocationItemFactory;
    protected readonly IUrlsBrowser UrlsBrowser;
    protected readonly IWebAuthenticator WebAuthenticator;

    public abstract CountriesConnectionType ConnectionType { get; }

    public abstract int SortIndex { get; }

    public abstract string Header { get; }

    public abstract string Description { get; }

    public abstract bool IsInfoBannerVisible { get; }

    protected abstract ModalSource UpsellModalSource { get; }

    public bool IsUpsellBannerVisible => IsRestricted;

    public bool IsRestricted => !Settings.VpnPlan.IsPaid;

    protected CountriesComponentViewModelBase(
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IServersLoader serversLoader,
        ILocationItemFactory locationItemFactory,
        IUrlsBrowser urlsBrowser,
        IWebAuthenticator webAuthenticator)
        : base(localizer, logger, issueReporter)
    {
        Settings = settings;
        ServersLoader = serversLoader;
        LocationItemFactory = locationItemFactory;

        UrlsBrowser = urlsBrowser;
        WebAuthenticator = webAuthenticator;
    }

    public virtual IEnumerable<ConnectionItemBase> GetItems()
    {
        IEnumerable<ConnectionItemBase> genericCountries =
        [
            LocationItemFactory.GetGenericCountry(ConnectionType, ConnectionIntentKind.Fastest, false),

            // Do not include 'Fastest (excluding my country)' and 'Random country' in the options
            //LocationItemFactory.GetGenericCountry(ConnectionType, ConnectionIntentKind.Fastest, true),
            //LocationItemFactory.GetGenericCountry(ConnectionType, ConnectionIntentKind.Random, false),
        ];

        return genericCountries;
    }

    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() => OnSettingsChanged(message.PropertyName));
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    [RelayCommand]
    protected abstract void DismissInfoBanner();

    [RelayCommand]
    private async Task UpgradeAsync()
    {
        UrlsBrowser.BrowseTo(await WebAuthenticator.GetUpgradeAccountUrlAsync(UpsellModalSource));
    }

    protected virtual void OnSettingsChanged(string propertyName)
    { }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(Description));
    }
}