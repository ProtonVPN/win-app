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

using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.FeatureIcons;

public abstract class FeatureIconViewModelBase : ViewModelBase,
    IEventMessageReceiver<ThemeChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly IConnectionManager _connectionManager;

    public ImageSource Icon => GetImageSource();

    public virtual bool IsDimmed => IsFeatureEnabled && !_connectionManager.IsConnected;

    protected abstract bool IsFeatureEnabled { get; }

    protected FeatureIconViewModelBase(
        IConnectionManager connectionManager,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizer, logger, issueReporter)
    {
        _connectionManager = connectionManager;
    }

    public void Receive(ThemeChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateIcon);
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
            ExecuteOnUIThread(() =>
            {
                InvalidateIcon();
                InvalidateIsDimmed();
            });
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateIsDimmed);
    }

    protected abstract ImageSource GetImageSource();

    protected abstract IEnumerable<string> GetSettingsChangedForIconUpdate();

    private void InvalidateIcon()
    {
        OnPropertyChanged(nameof(Icon));
    }

    private void InvalidateIsDimmed()
    {
        OnPropertyChanged(nameof(IsDimmed));
    }
}