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

using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;

namespace ProtonVPN.Client.UI.Settings.Pages;

public class AutoStartupViewModel : PageViewModelBase<IMainViewNavigator>, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ISettings _settings;

    public bool IsAutoLaunchEnabled
    {
        get => _settings.IsAutoLaunchEnabled;
        set => _settings.IsAutoLaunchEnabled = value;
    }

    public bool IsAutoLaunchOpenOnDesktop
    {
        get => IsAutoLaunchMode(AutoLaunchMode.OpenOnDesktop);
        set => SetAutoLaunchMode(value, AutoLaunchMode.OpenOnDesktop);
    }

    public bool IsAutoLaunchMinimizeToSystemTray
    {
        get => IsAutoLaunchMode(AutoLaunchMode.MinimizeToSystemTray);
        set => SetAutoLaunchMode(value, AutoLaunchMode.MinimizeToSystemTray);
    }

    public bool IsAutoLaunchMinimizeToTaskbar
    {
        get => IsAutoLaunchMode(AutoLaunchMode.MinimizeToTaskbar);
        set => SetAutoLaunchMode(value, AutoLaunchMode.MinimizeToTaskbar);
    }

    public bool IsAutoConnectEnabled
    {
        get => _settings.IsAutoConnectEnabled;
        set => _settings.IsAutoConnectEnabled = value;
    }

    public bool IsAutoConnectFastestConnection
    {
        get => IsAutoConnectMode(AutoConnectMode.FastestConnection);
        set => SetAutoConnectMode(value, AutoConnectMode.FastestConnection);
    }

    public bool IsAutoConnectLatestConnection
    {
        get => IsAutoConnectMode(AutoConnectMode.LatestConnection);
        set => SetAutoConnectMode(value, AutoConnectMode.LatestConnection);
    }

    public override string? Title => Localizer.Get("Settings_General_AutoStartup");

    public AutoStartupViewModel(IMainViewNavigator viewNavigator, ILocalizationProvider localizationProvider, ISettings settings)
        : base(viewNavigator, localizationProvider)
    {
        _settings = settings;
    }

    public void Receive(SettingChangedMessage message)
    {
        switch (message.PropertyName)
        {
            case nameof(ISettings.IsAutoLaunchEnabled):
                OnPropertyChanged(nameof(IsAutoLaunchEnabled));
                break;

            case nameof(ISettings.AutoLaunchMode):
                OnPropertyChanged(nameof(IsAutoLaunchOpenOnDesktop));
                OnPropertyChanged(nameof(IsAutoLaunchMinimizeToSystemTray));
                OnPropertyChanged(nameof(IsAutoLaunchMinimizeToTaskbar));
                break;

            case nameof(ISettings.IsAutoConnectEnabled):
                OnPropertyChanged(nameof(IsAutoConnectEnabled));
                break;

            case nameof(ISettings.AutoConnectMode):
                OnPropertyChanged(nameof(IsAutoConnectFastestConnection));
                OnPropertyChanged(nameof(IsAutoConnectLatestConnection));
                break;
        }
    }

    private bool IsAutoLaunchMode(AutoLaunchMode autoLaunchMode)
    {
        return _settings.AutoLaunchMode == autoLaunchMode;
    }

    private void SetAutoLaunchMode(bool value, AutoLaunchMode autoLaunchMode)
    {
        if (value)
        {
            _settings.AutoLaunchMode = autoLaunchMode;
        }
    }

    private bool IsAutoConnectMode(AutoConnectMode autoConnectMode)
    {
        return _settings.AutoConnectMode == autoConnectMode;
    }

    private void SetAutoConnectMode(bool value, AutoConnectMode autoConnectMode)
    {
        if (value)
        {
            _settings.AutoConnectMode = autoConnectMode;
        }
    }
}