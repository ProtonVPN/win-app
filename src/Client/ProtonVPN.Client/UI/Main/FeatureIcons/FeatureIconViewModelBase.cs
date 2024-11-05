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
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.FeatureIcons;

public abstract class FeatureIconViewModelBase : ViewModelBase,
    IEventMessageReceiver<ThemeChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<ConnectionStatusChanged>
{
    private readonly IConnectionManager _connectionManager;

    public ImageSource Icon => GetImageSource();

    public bool IsDimmed => IsFeatureEnabled && !_connectionManager.IsConnected;

    protected FeatureIconViewModelBase(
        IConnectionManager connectionManager,
        ILocalizationProvider localizer,
        ILogger logger, IIssueReporter
        issueReporter) : base(localizer, logger, issueReporter)
    {
        _connectionManager = connectionManager;
    }

    protected abstract ImageSource GetImageSource();

    protected abstract IEnumerable<string> GetSettingsChangedForIconUpdate();

    protected abstract bool IsFeatureEnabled { get; }

    public void Receive(ThemeChangedMessage message)
    {
        OnPropertyChanged(nameof(Icon));
    }

    public void Receive(SettingChangedMessage message)
    {
        if (GetSettingsChangedForIconUpdate().Contains(message.PropertyName))
        {
            OnPropertyChanged(nameof(Icon));
            OnPropertyChanged(nameof(IsDimmed));
        }
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(IsDimmed));
        });
    }
}