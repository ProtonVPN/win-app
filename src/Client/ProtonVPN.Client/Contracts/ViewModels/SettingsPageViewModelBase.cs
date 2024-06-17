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

using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Conflicts.Bases;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Settings.Pages.Entities;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Contracts.ViewModels;

public abstract partial class SettingsPageViewModelBase : PageViewModelBase<IMainViewNavigator>,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    protected readonly IOverlayActivator OverlayActivator;
    protected readonly ISettings Settings;
    protected readonly ISettingsConflictResolver SettingsConflictResolver;
    protected readonly IConnectionManager ConnectionManager;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    private bool _isPageReady = false;

    public string ApplyCommandText => Localizer.Get(IsReconnectionRequired()
        ? "Common_Actions_Reconnect"
        : "Settings_Common_Apply");

    protected SettingsPageViewModelBase(IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IOverlayActivator overlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(viewNavigator,
               localizationProvider,
               logger,
               issueReporter)
    {
        OverlayActivator = overlayActivator;
        Settings = settings;
        SettingsConflictResolver = settingsConflictResolver;
        ConnectionManager = connectionManager;
    }

    [RelayCommand(CanExecute = nameof(CanApply))]
    public async Task ApplyAsync()
    {
        bool isReconnectionRequired = IsReconnectionRequired();

        await SaveSettingsAsync();

        if (isReconnectionRequired)
        {
            await ConnectionManager.ReconnectAsync();
        }
    }

    public bool CanApply()
    {
        return IsPageReady && HasChangedSettings();
    }

    public void Receive(ConnectionStatusChanged message)
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

    public override async Task<bool> OnNavigatingFromAsync(bool forceNavigation = false)
    {
        if (forceNavigation)
        {
            return true;
        }

        if (!CanApply()) // No changes made, simply leave page
        {
            return true;
        }

        ContentDialogResult result = await OverlayActivator.ShowMessageAsync(
            new MessageDialogParameters
            {
                Title = Localizer.Get("Settings_Common_Discard_Title"),
                PrimaryButtonText = Localizer.Get("Settings_Common_Discard"),
                SecondaryButtonText = Localizer.Get(IsReconnectionRequired()
                    ? "Settings_Common_ApplyAndReconnect"
                    : "Settings_Common_Apply"),
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
                UseVerticalLayoutForButtons = true,
            });

        switch (result)
        {
            case ContentDialogResult.Primary: // Do nothing, user decided to discard settings changes.
                return true;

            case ContentDialogResult.Secondary: // Apply settings and trigger reconnection if needed
                await ApplyAsync();
                return true;

            default: // Cancel navigation, stays on current page without deleting changes user have made
                return false;
        }
    }

    public override async void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        base.OnNavigatedTo(parameter, isBackNavigation);

        await RetrieveSettingsAsync();
    }

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();

        // Reset flag when navigating to another page
        IsPageReady = false;
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(async () => await RetrieveSettingsAsync());
    }

    protected abstract IEnumerable<ChangedSettingArgs> GetSettings();

    protected virtual void OnConnectionStatusChanged(ConnectionStatus connectionStatus)
    { }

    protected virtual void OnSettingsChanged(string propertyName)
    { }

    private async Task SaveSettingsAsync()
    {
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

        string settingName = GetSettingName(e.PropertyName);
        object? settingValue = GetType()?.GetProperty(e.PropertyName)?.GetValue(this);

        ISettingsConflict? conflict = SettingsConflictResolver.GetConflict(settingName, settingValue);

        if (conflict != null)
        {
            ContentDialogResult result = await OverlayActivator.ShowMessageAsync(conflict.MessageParameters);
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
        foreach (ChangedSettingArgs changedSetting in changedSettings)
        {
            if (RequiredReconnectionSettings.Contains(changedSetting.Name))
            {
                return true;
            }

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
        return GetSettings().Where(s => s.HasChanged);
    }

    private string GetSettingName(string propertyName)
    {
        PropertyInfo? propertyInfo = GetType()?.GetProperty(propertyName);

        return propertyInfo != null
            && Attribute.GetCustomAttribute(propertyInfo, typeof(SettingNameAttribute)) is SettingNameAttribute attribute
            && !string.IsNullOrEmpty(attribute.SettingPropertyName)
            ? attribute.SettingPropertyName
            : propertyName;
    }
}