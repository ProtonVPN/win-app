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
using ProtonVPN.Client.Contracts.Profiles;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Conflicts.Bases;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Client.UI.Main.Widgets.Bases;
using ProtonVPN.Client.UI.Main.Widgets.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.UI.Main.Features.Bases;

public abstract partial class FeatureWidgetViewModelBase : SideWidgetViewModelBase, ISideHeaderWidget,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<ProfilesChangedMessage>
{
    protected readonly ISettingsViewNavigator SettingsViewNavigator;
    protected readonly IMainWindowOverlayActivator MainWindowOverlayActivator;
    protected readonly ISettings Settings;
    protected readonly IConnectionManager ConnectionManager;
    protected readonly IUpsellCarouselWindowActivator UpsellCarouselWindowActivator;
    protected readonly IRequiredReconnectionSettings RequiredReconnectionSettings;
    protected readonly ISettingsConflictResolver SettingsConflictResolver;
    protected readonly IProfileEditor ProfileEditor;

    [ObservableProperty]
    private bool _isFeaturePageDisplayed;

    [ObservableProperty]
    private bool _isFeatureFlyoutOpened;

    public override int SortIndex => (int)ConnectionFeature;

    public string Status => GetFeatureStatus();

    public ConnectionFeature ConnectionFeature { get; }

    public virtual bool IsRestricted => !Settings.VpnPlan.IsPaid;

    protected abstract UpsellFeatureType? UpsellFeature { get; }

    public virtual bool IsFeatureOverridden => false;

    public IConnectionProfile? CurrentProfile => ConnectionManager.CurrentConnectionIntent as IConnectionProfile;

    protected FeatureWidgetViewModelBase(
        IViewModelHelper viewModelHelper,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        ISettingsConflictResolver settingsConflictResolver,
        IProfileEditor profileEditor,
        ConnectionFeature connectionFeature)
        : base(mainViewNavigator, viewModelHelper)
    {
        SettingsViewNavigator = settingsViewNavigator;
        MainWindowOverlayActivator = mainWindowOverlayActivator;
        Settings = settings;
        ConnectionManager = connectionManager;
        UpsellCarouselWindowActivator = upsellCarouselWindowActivator;
        RequiredReconnectionSettings = requiredReconnectionSettings;
        SettingsConflictResolver = settingsConflictResolver;
        ProfileEditor = profileEditor;

        ConnectionFeature = connectionFeature;

        SettingsViewNavigator.Navigated += OnSettingsViewNavigation;
    }

    public override async Task<bool> InvokeAsync()
    {
        if (IsRestricted)
        {
            return await UpsellCarouselWindowActivator.ActivateAsync(UpsellFeature);
        }

        if (IsFeatureOverridden && CurrentProfile != null)
        {
            return await ProfileEditor.TryRedirectToProfileAsync(Header, CurrentProfile);
        }

        return await MainViewNavigator.NavigateToSettingsViewAsync() &&
               await SettingsViewNavigator.NavigateToFeatureViewAsync(ConnectionFeature, isDirectNavigation: true);
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
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    public void Receive(ProfilesChangedMessage message)
    {
        if (ConnectionManager.IsConnected)
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
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

    protected abstract bool IsOnFeaturePage(PageViewModelBase? currentPageContext);

    protected abstract void OnSettingsChanged();

    protected abstract void OnConnectionStatusChanged();

    protected Task<bool> TryChangeFeatureSettingsAsync(Lazy<List<ChangedSettingArgs>> changedSettings)
    {
        return TryChangeFeatureSettingsAsync(changedSettings.Value);
    }

    protected async Task<bool> TryChangeFeatureSettingsAsync(List<ChangedSettingArgs> changedSettings)
    {
        List<ISettingsConflict> conflicts = new();

        foreach (ChangedSettingArgs changedSetting in changedSettings)
        {
            ISettingsConflict? conflict = SettingsConflictResolver.GetConflict(changedSetting.Name, changedSetting.NewValue);
            if (conflict != null)
            {
                ContentDialogResult result = await MainWindowOverlayActivator.ShowMessageAsync(conflict.MessageParameters);
                if (result != ContentDialogResult.Primary)
                {
                    return false;
                }

                conflicts.Add(conflict);
            }
        }

        bool isReconnectionRequired = IsReconnectionRequired(changedSettings, conflicts);
        if (isReconnectionRequired)
        {
            ContentDialogResult result = await MainWindowOverlayActivator.ShowMessageAsync(new()
            {
                Title = Localizer.Get("Settings_Reconnection_Title"),
                PrimaryButtonText = Localizer.Get("Common_Actions_Reconnect"),
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
            });
            if (result != ContentDialogResult.Primary)
            {
                return false;
            }
        }

        foreach (ChangedSettingArgs settings in changedSettings)
        {
            settings.ApplyChanges();
        }

        if (isReconnectionRequired)
        {
            await ConnectionManager.ReconnectAsync(VpnTriggerDimension.NewConnection);
        }

        return true;
    }

    private bool IsOnSettingsPage()
    {
        return MainViewNavigator.GetCurrentPageContext() is SettingsPageViewModel;
    }

    private void OnSettingsViewNavigation(object sender, NavigationEventArgs e)
    {
        InvalidateIsSelected();
    }

    partial void OnIsFeatureFlyoutOpenedChanged(bool value)
    {
        InvalidateIsSelected();
    }

    private bool IsReconnectionRequired(List<ChangedSettingArgs> changedSettings, List<ISettingsConflict> conflicts)
    {
        if (ConnectionManager.IsDisconnected)
        {
            return false;
        }

        return IsReconnectionRequiredDueToChanges(changedSettings)
            || IsReconnectionRequiredDueToConflicts(conflicts);
    }

    private bool IsReconnectionRequiredDueToChanges(IEnumerable<ChangedSettingArgs> changedSettings)
    {
        return changedSettings.Any(s => RequiredReconnectionSettings.IsReconnectionRequired(s.Name));
    }

    private bool IsReconnectionRequiredDueToConflicts(List<ISettingsConflict> conflicts)
    {
        return conflicts.Any(c => c.IsReconnectionRequired);
    }
}