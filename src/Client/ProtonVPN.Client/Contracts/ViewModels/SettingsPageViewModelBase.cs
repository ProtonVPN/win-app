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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Conflicts.Bases;
using ProtonVPN.Client.Settings.Contracts.Messages;

namespace ProtonVPN.Client.Contracts.ViewModels;

public abstract partial class SettingsPageViewModelBase : PageViewModelBase<IMainViewNavigator>, IEventMessageReceiver<SettingChangedMessage>
{
    protected readonly ISettings Settings;
    private readonly ISettingsConflictResolver _settingsConflictResolver;

    public SettingsPageViewModelBase(IMainViewNavigator viewNavigator, ILocalizationProvider localizationProvider, ISettings settings, ISettingsConflictResolver settingsConflictResolver)
        : base(viewNavigator, localizationProvider)
    {
        Settings = settings;
        _settingsConflictResolver = settingsConflictResolver;
    }

    public void Receive(SettingChangedMessage message)
    {
        OnSettingsChanged(message.PropertyName);
    }

    public override Task<bool> OnNavigatingFromAsync()
    {
        if (HasConfigurationChanged())
        {
            // If any changes, save settings when leaving page
            SaveSettings();
        }

        return Task.FromResult(true);
    }

    public override void OnNavigatedTo(object parameter)
    {
        base.OnNavigatedTo(parameter);

        RetrieveSettings();
    }

    protected virtual void OnSettingsChanged(string propertyName)
    { }

    protected abstract bool HasConfigurationChanged();

    protected abstract void SaveSettings();

    protected abstract void RetrieveSettings();

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (string.IsNullOrEmpty(e?.PropertyName))
        {
            return;
        }

        string settingName = GetSettingName(e.PropertyName);
        object? settingValue = GetType()?.GetProperty(e.PropertyName)?.GetValue(this);

        ISettingsConflict? conflict = _settingsConflictResolver.GetConflict(settingName, settingValue);

        if (conflict != null)
        {
            ContentDialogResult result = await ViewNavigator.ShowMessageAsync(conflict.MessageParameters);
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