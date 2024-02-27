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
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
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
    IEventMessageReceiver<SettingChangedMessage>
{
    protected readonly IOverlayActivator OverlayActivator;
    protected readonly ISettings Settings;
    protected readonly ISettingsConflictResolver SettingsConflictResolver;
    protected readonly IConnectionManager ConnectionManager;

    public SettingsPageViewModelBase(IMainViewNavigator viewNavigator,
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

    [RelayCommand(CanExecute = nameof(CanReconnect))]
    public async Task ReconnectAsync()
    {
        SaveSettings();

        await ConnectionManager.ReconnectAsync();
    }

    public bool CanReconnect()
    {
        return !ConnectionManager.IsDisconnected
            && HasChangedSettings()
            && IsReconnectionRequired();
    }

    private bool HasChangedSettings()
    {
        return GetChangedSettings().Any();
    }

    protected abstract IEnumerable<ChangedSettingArgs> GetSettings();

    private bool IsReconnectionRequired()
    {
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

    public virtual void Receive(ConnectionStatusChanged message)
    {
        ExecuteOnUIThread(() =>
        {
            ReconnectCommand.NotifyCanExecuteChanged();
        });
    }

    public virtual void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnSettingsChanged(message.PropertyName);
        });
    }

    public override async Task<bool> OnNavigatingFromAsync()
    {
        if (!CanReconnect()) // No reconnection required, simply save settings when leaving page
        {
            if (HasChangedSettings()) // If any changes, save settings when leaving page
            {
                SaveSettings();
            }
            return true;
        }

        ContentDialogResult result = await OverlayActivator.ShowMessageAsync(
            new MessageDialogParameters
            {
                Title = Localizer.Get("Settings_Common_Discard_Title"),
                PrimaryButtonText = Localizer.Get("Settings_Common_Discard"),
                SecondaryButtonText = Localizer.Get("Settings_Common_ApplyAndReconnect"),
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
                UseVerticalLayoutForButtons = true,
            });

        switch (result)
        {
            case ContentDialogResult.Primary: // Do nothing, user decided to discard settings changes.
                return true;

            case ContentDialogResult.Secondary: // Apply settings and trigger reconnections
                await ReconnectAsync();
                return true;

            default: // Cancel navigation, stays on current page without deleting changes user have made
                return false;
        }
    }

    public override void OnNavigatedTo(object parameter)
    {
        base.OnNavigatedTo(parameter);

        RetrieveSettings();
    }

    protected virtual void OnSettingsChanged(string propertyName)
    { }

    protected abstract void SaveSettings();

    protected abstract void RetrieveSettings();

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        ReconnectCommand.NotifyCanExecuteChanged();

        if (string.IsNullOrEmpty(e?.PropertyName))
        {
            return;
        }

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