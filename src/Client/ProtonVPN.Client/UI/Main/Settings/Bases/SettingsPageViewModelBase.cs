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

using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Conflicts.Bases;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.UI.Main;

public abstract partial class SettingsPageViewModelBase : PageViewModelBase<ISettingsViewNavigator>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    protected readonly IRequiredReconnectionSettings RequiredReconnectionSettings;
    protected readonly IMainViewNavigator MainViewNavigator;
    protected readonly IMainWindowOverlayActivator MainWindowOverlayActivator;
    protected readonly ISettings Settings;
    protected readonly ISettingsConflictResolver SettingsConflictResolver;
    protected readonly IConnectionManager ConnectionManager;

    private bool _isNavigationFromHomePage = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    private bool _isPageReady;

    public string ApplyCommandText => Localizer.Get(IsReconnectionRequired()
        ? "Common_Actions_Reconnect"
        : "Settings_Common_Apply");

    protected IEnumerable<ChangedSettingArgs> PageSettings { get; set; } = [];

    protected SettingsPageViewModelBase(
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper)
        : base(settingsViewNavigator, viewModelHelper)
    {
        RequiredReconnectionSettings = requiredReconnectionSettings;
        MainViewNavigator = mainViewNavigator;
        MainWindowOverlayActivator = mainWindowOverlayActivator;
        Settings = settings;
        SettingsConflictResolver = settingsConflictResolver;
        ConnectionManager = connectionManager;
    }

    [RelayCommand]
    public async Task<bool> CloseAsync()
    {
        bool navigationCompleted = 
            await ParentViewNavigator.NavigateToDefaultAsync() &&
            await MainViewNavigator.NavigateToHomeViewAsync();

        if (navigationCompleted)
        {
            RequestResetContentScroll();
        }

        return navigationCompleted;
    }

    [RelayCommand(CanExecute = nameof(CanApply))]
    public async Task<bool> ApplyAsync()
    {
        return await ApplyChangesAsync()
            && _isNavigationFromHomePage
                ? await CloseAsync()
                : await ParentViewNavigator.NavigateToDefaultAsync();
    }

    public bool CanApply()
    {
        return IsPageReady && HasChangedSettings();
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(ApplyCommandText));
            OnConnectionStatusChanged(message.ConnectionStatus);
        });
    }

    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnSettingsChanged(message.PropertyName);
        });
    }

    public override async void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        base.OnNavigatedTo(parameter, isBackNavigation);

        _isNavigationFromHomePage = Convert.ToBoolean(parameter ?? false);

        await RetrieveSettingsAsync();
    }

    public override async Task<bool> CanNavigateFromAsync()
    {
        if (!CanApply()) // No changes made, simply leave page
        {
            return true;
        }

        ContentDialogResult result = await MainWindowOverlayActivator.ShowDiscardConfirmationOverlayAsync();
        switch (result)
        {
            case ContentDialogResult.Primary: // Do nothing, user decided to discard settings changes.
                return true;

            case ContentDialogResult.Secondary: // Apply settings and trigger reconnection if needed
                return await ApplyChangesAsync();

            default: // Cancel navigation, stays on current page without deleting changes user have made
                return false;
        }
    }

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();

        // Reset flag when navigating to another page
        IsPageReady = false;
        _isNavigationFromHomePage = false;
    }

    protected virtual void OnConnectionStatusChanged(ConnectionStatus connectionStatus)
    { }

    protected virtual void OnSettingsChanged(string propertyName)
    { }

    protected virtual void OnIpv6WarningClosedWithPrimaryAction()
    { }

    protected async Task ShowIpv6DisabledWarningAsync()
    {
        ContentDialogResult result = await MainWindowOverlayActivator.ShowMessageAsync(new()
        {
            Title = Localizer.Get("Overlay_Ipv6Disabled_Title"),
            Message = Localizer.Get("Overlay_Ipv6Disabled_Description"),
            PrimaryButtonText = Localizer.Get("Overlay_Ipv6Disabled_PrimaryButton"),
            SecondaryButtonText = Localizer.Get("Overlay_Ipv6Disabled_SecondaryButton"),
        });

        if (result == ContentDialogResult.Primary)
        {
            Settings.IsIpv6Enabled = true;
            OnIpv6WarningClosedWithPrimaryAction();
        }
    }

    private async Task SaveSettingsAsync()
    {
        IEnumerable<ChangedSettingArgs> changedSettings = GetChangedSettings();
        foreach (ChangedSettingArgs setting in changedSettings)
        {
            setting.ApplyChanges();
        }

        OnSaveSettings();
        await OnSaveSettingsAsync();
    }

    protected virtual void OnSaveSettings()
    { }

    protected virtual Task OnSaveSettingsAsync()
    {
        return Task.CompletedTask;
    }

    private async Task RetrieveSettingsAsync()
    {
        try
        {
            // Keep flag off while retrieving settings
            IsPageReady = false;

            OnRetrieveSettings();
            await OnRetrieveSettingsAsync();
        }
        finally
        {
            IsPageReady = true;
        }
    }

    protected virtual void OnRetrieveSettings()
    { }

    protected virtual Task OnRetrieveSettingsAsync()
    {
        return Task.CompletedTask;
    }

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (string.IsNullOrEmpty(e?.PropertyName) || e.PropertyName == nameof(ApplyCommandText))
        {
            return;
        }

        ApplyCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(ApplyCommandText));

        string settingName = SettingNameAttribute.GetSettingName(this, e.PropertyName);
        object? settingValue = GetType()?.GetProperty(e.PropertyName)?.GetValue(this);

        ISettingsConflict? conflict = SettingsConflictResolver.GetConflict(settingName, settingValue);

        if (conflict != null)
        {
            ContentDialogResult result = await MainWindowOverlayActivator.ShowMessageAsync(conflict.MessageParameters);
            if (result != ContentDialogResult.Primary)
            {
                GetType()?.GetProperty(e.PropertyName)?.SetValue(this, conflict.SettingsResetValue);
            }
        }
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(ApplyCommandText));
    }

    private async Task<bool> ApplyChangesAsync()
    {
        bool isReconnectionRequired = IsReconnectionRequired();

        await SaveSettingsAsync();

        if (isReconnectionRequired)
        {
            return await ConnectionManager.ReconnectAsync(VpnTriggerDimension.NewConnection);
        }

        return true;
    }

    private bool HasChangedSettings()
    {
        return GetChangedSettings().Any();
    }

    private bool IsReconnectionRequired()
    {
        if (ConnectionManager.IsDisconnected)
        {
            return false;
        }

        IEnumerable<ChangedSettingArgs> changedSettings = GetChangedSettings();

        return IsReconnectionRequiredDueToChanges(changedSettings)
            || IsReconnectionRequiredDueToConflicts(changedSettings);
    }

    protected virtual bool IsReconnectionRequiredDueToChanges(IEnumerable<ChangedSettingArgs> changedSettings)
    {
        return changedSettings.Any(s => RequiredReconnectionSettings.IsReconnectionRequired(s.Name));
    }

    private bool IsReconnectionRequiredDueToConflicts(IEnumerable<ChangedSettingArgs> changedSettings)
    {
        foreach (ChangedSettingArgs changedSetting in changedSettings)
        {
            ISettingsConflict? conflict = SettingsConflictResolver.GetConflict(changedSetting.Name, changedSetting.NewValue);
            if (conflict is not null && conflict.IsReconnectionRequired)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerable<ChangedSettingArgs> GetChangedSettings()
    {
        return PageSettings.Where(s => s.HasChanged());
    }
}