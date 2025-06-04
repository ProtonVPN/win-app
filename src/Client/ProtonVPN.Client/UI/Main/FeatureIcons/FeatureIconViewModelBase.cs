/*
 * Copyright (c) 2025 Proton AG
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

using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;

namespace ProtonVPN.Client.UI.Main.FeatureIcons;

public abstract class FeatureIconViewModelBase : ViewModelBase,
    IEventMessageReceiver<ThemeChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<ProfilesChangedMessage>
{
    protected readonly IConnectionManager ConnectionManager;
    protected readonly ISettings Settings;
    protected readonly IApplicationThemeSelector ThemeSelector;

    public ImageSource Icon => GetImageSource();

    public virtual bool IsDimmed => IsFeatureEnabled && !ConnectionManager.IsConnected;

    protected abstract bool IsFeatureEnabled { get; }

    protected IConnectionProfile? CurrentProfile => ConnectionManager.CurrentConnectionIntent as IConnectionProfile;

    protected FeatureIconViewModelBase(
        IConnectionManager connectionManager,
        ISettings settings,
        IApplicationThemeSelector themeSelector,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        ConnectionManager = connectionManager;
        Settings = settings;
        ThemeSelector = themeSelector;
    }

    public void Receive(ThemeChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    public void Receive(SettingChangedMessage message)
    {
        if (GetSettingsChangedForIconUpdate().Contains(message.PropertyName))
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
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

    protected abstract ImageSource GetImageSource();

    protected abstract IEnumerable<string> GetSettingsChangedForIconUpdate();
}