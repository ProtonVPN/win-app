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

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.UI.Main.Settings;
using ProtonVPN.Client.UI.Main.Settings.Connection;
using ProtonVPN.Client.UI.Main.Widgets.Bases;
using ProtonVPN.Client.UI.Main.Widgets.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Features.Bases;

public abstract partial class FeatureWidgetViewModelBase : SideWidgetViewModelBase, ISideHeaderWidget,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>
{
    protected readonly ISettingsViewNavigator SettingsViewNavigator;
    protected readonly ISettings Settings;
    protected readonly IConnectionManager ConnectionManager;
    protected readonly IUpsellCarouselWindowActivator UpsellCarouselWindowActivator;

    [ObservableProperty]
    private bool _isFeaturePageDisplayed;

    [ObservableProperty]
    private bool _isFeatureFlyoutOpened;

    public override int SortIndex => (int)ConnectionFeature;

    public string Status => GetFeatureStatus();

    public ConnectionFeature ConnectionFeature { get; }

    public virtual bool IsRestricted => !Settings.VpnPlan.IsPaid;

    protected FeatureWidgetViewModelBase(
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        ISettings settings,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        ConnectionFeature connectionFeature)
        : base(localizer, logger, issueReporter, mainViewNavigator)
    {
        SettingsViewNavigator = settingsViewNavigator;
        SettingsViewNavigator.Navigated += OnSettingsViewNavigation;
        Settings = settings;
        ConnectionManager = connectionManager;
        UpsellCarouselWindowActivator = upsellCarouselWindowActivator;

        ConnectionFeature = connectionFeature;
    }

    public override async Task<bool> InvokeAsync()
    {
        if (IsRestricted)
        {
            UpsellCarouselWindowActivator.Activate();
            return false;
        }

        return await MainViewNavigator.NavigateToSettingsViewAsync() &&
               await SettingsViewNavigator.NavigateToFeatureViewAsync(ConnectionFeature);
    }

    public void Receive(SettingChangedMessage message)
    {
        if (GetSettingsChangedForUpdate().Contains(message.PropertyName))
        {
            ExecuteOnUIThread(OnSettingsChanged);
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(OnConnectionStatusChanged);
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(IsRestricted)));
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    protected abstract IEnumerable<string> GetSettingsChangedForUpdate();

    protected abstract string GetFeatureStatus();

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Status));
    }

    protected override void InvalidateIsSelected()
    {
        IsFeaturePageDisplayed = IsOnSettingsPage()
                              && IsOnFeaturePage(SettingsViewNavigator.GetCurrentPageContext());

        IsSelected = IsFeaturePageDisplayed || IsFeatureFlyoutOpened;
    }

    private bool IsOnSettingsPage()
    {
        return MainViewNavigator.GetCurrentPageContext() is SettingsPageViewModel;
    }

    protected abstract bool IsOnFeaturePage(PageViewModelBase? currentPageContext);

    protected abstract void OnSettingsChanged();

    protected abstract void OnConnectionStatusChanged();

    private void OnSettingsViewNavigation(object sender, NavigationEventArgs e)
    {
        InvalidateIsSelected();
    }

    partial void OnIsFeatureFlyoutOpenedChanged(bool value)
    {
        InvalidateIsSelected();
    }
}